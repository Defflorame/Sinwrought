using UnityEngine;
using Game.Combat;
using Game.Items;
using Game.Magic;
using Game.Player.Equipment;

namespace Game.Player
{
    /// <summary>
    /// Координатор боя игрока. Не рассчитывает урон самостоятельно.
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private PlayerActionStateMachine stateMachine;
        [SerializeField] private WeaponAttackController weaponAttackController;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private StaminaComponent stamina;
        [SerializeField] private BlockResolver blockResolver;
        [SerializeField] private ParryResolver parryResolver;

        [Header("Magic")]
        [SerializeField] private SpellCaster spellCaster;
        [SerializeField] private RingSpellController ringSpellController;

        [Header("Equipment")]
        [SerializeField] private CombatLoadout combatLoadout;

        [Header("Attack tuning")]
        [Tooltip("Порог удержания ЛКМ: короче — Light Attack, дольше — начинается заряд (Heavy Attack).")]
        [SerializeField] private float chargeHoldThreshold = 0.25f;

        [Header("Dodge tuning")]
        [SerializeField] private float dodgeStaminaCost = 20f;
        [SerializeField] private float dodgeDuration = 0.3f;

        [Header("Parry tuning")]
        [SerializeField] private float parryStaminaCost = 10f;

        private FreeState _freeState;
        private AttackState _attackState;
        private ChargeAttackState _chargeAttackState;
        private BlockState _blockState;
        private DodgeState _dodgeState;
        private ParryState _parryState;
        private CastState _castState;
        private StaggerState _staggerState;

        private bool _attackButtonHeld;
        private float _attackPressTime;
        private bool _chargeStarted;

        private void Awake()
        {
            _freeState = new FreeState();
            _attackState = new AttackState(weaponAttackController, ReturnToFree);
            _chargeAttackState = new ChargeAttackState(weaponAttackController, TrySpendStamina, ReturnToFree);
            _blockState = new BlockState(blockResolver, combatLoadout);
            _dodgeState = new DodgeState(playerMovement, combatLoadout, dodgeDuration, ReturnToFree);
            _parryState = new ParryState(parryResolver, ReturnToFree);
            _castState = new CastState(spellCaster, ringSpellController, ReturnToFree);
            _staggerState = new StaggerState(playerMovement, ReturnToFree);

        }

        private void Start()
        {
            stateMachine.ChangeState(_freeState);
        }

        private void Update()
        {
            if (!_attackButtonHeld || _chargeStarted) return;

            if (!CanPerformAction())
            {
                _attackButtonHeld = false;
                return;
            }

            if (Time.time - _attackPressTime >= chargeHoldThreshold)
            {
                _chargeStarted = true;
                Debug.Log("PlayerCombatController: удержание превысило порог — переход в ChargeAttackState");
                stateMachine.ChangeState(_chargeAttackState);
            }
        }

        public void OnAttackPressed()
        {
            if (!CanPerformAction())
            {
                Debug.Log("PlayerCombatController: Attack press проигнорирован — текущее состояние не прерываемо");
                return;
            }

            // Атака отменяет заряд заклинания сразу при нажатии ЛКМ (бесплатно),
            // а не после её отпускания — как блок, отскок и парирование.
            if (IsChargingSpell)
            {
                ReturnToFree();
            }

            _attackButtonHeld = true;
            _attackPressTime = Time.time;
            _chargeStarted = false;
        }

        public void OnAttackReleased()
        {
            if (!_attackButtonHeld) return;
            _attackButtonHeld = false;

            if (_chargeStarted)
            {
                _chargeAttackState.NotifyReleased();
            }
            else
            {
                TryLightAttack();
            }
        }

        public void StartBlockOrDodge()
        {
            if (combatLoadout.HasShield)
            {
                if (!CanPerformAction())
                {
                    Debug.Log("PlayerCombatController: Block проигнорирован — текущее состояние не прерываемо");
                    return;
                }

                stateMachine.ChangeState(_blockState);
            }
            else
            {
                TryDodge();
            }
        }

        public void StopBlock()
        {
            if (stateMachine.CurrentState == _blockState)
            {
                stateMachine.ChangeState(_freeState);
            }
        }

        public void TryDodge()
        {
            if (!CanPerformAction())
            {
                Debug.Log("PlayerCombatController: Dodge проигнорирован — текущее состояние не прерываемо");
                return;
            }

            if (!playerMovement.IsGrounded)
            {
                Debug.Log("PlayerCombatController: Dodge недоступен в воздухе");
                return;
            }

            if (!TrySpendStamina(dodgeStaminaCost))
            {
                Debug.Log("PlayerCombatController: недостаточно stamina для Dodge");
                return;
            }

            stateMachine.ChangeState(_dodgeState);
        }

        public void TryParry()
        {
            if (!CanPerformAction())
            {
                Debug.Log("PlayerCombatController: Parry проигнорирован — текущее состояние не прерываемо");
                return;
            }

            if (!TrySpendStamina(parryStaminaCost))
            {
                Debug.Log("PlayerCombatController: недостаточно stamina для Parry");
                return;
            }

            stateMachine.ChangeState(_parryState);
        }

        /// <summary>Нажатие кнопки каста: начинает заряд (для castTime = 0 — сразу выпускает). Стоимость списывается только при выпуске.</summary>
        public void TryCastSpell()
        {
            if (!CanPerformAction())
            {
                Debug.Log("PlayerCombatController: Cast проигнорирован — текущее состояние не прерываемо");
                return;
            }

            SpellData spell = ringSpellController.ActiveSpell;
            if (spell == null)
            {
                Debug.Log("PlayerCombatController: Cast проигнорирован — нет кольца с заклинанием");
                return;
            }

            if (!spellCaster.CanCast(spell)) return;

            stateMachine.ChangeState(_castState);
        }

        /// <summary>Отпускание кнопки каста. Если заряд уже отменён другим действием — игнорируется.</summary>
        public void OnCastReleased()
        {
            if (stateMachine.CurrentState != _castState) return;

            _castState.NotifyReleased();
        }

        /// <summary>direction: +1 следующее кольцо, -1 предыдущее. Во время заряда заклинания игнорируется.</summary>
        public void SwitchRing(int direction)
        {
            if (IsChargingSpell) return;

            ringSpellController.SwitchRing(direction);
        }

        private bool IsChargingSpell => stateMachine.CurrentState == _castState && _castState.IsCharging;

        /// <summary>Переключает активную руку при двух одноручных оружиях (dual-wield).</summary>
        public void TrySwitchWeapon()
        {
            if (!CanPerformAction())
            {
                Debug.Log("PlayerCombatController: Switch weapon проигнорирован — текущее состояние не прерываемо");
                return;
            }

            combatLoadout.TrySwitchActiveWeapon();
        }

        public bool CanPerformAction()
        {
            return stateMachine.CurrentState == null || stateMachine.CurrentState.CanInterrupt();
        }

        private void TryLightAttack()
        {
            if (!CanPerformAction())
            {
                Debug.Log("PlayerCombatController: Light Attack проигнорирован — текущее состояние не прерываемо");
                return;
            }

            AttackData lightAttackData = weaponAttackController.LightAttackData;

            if (!lightAttackData)
            {
                Debug.Log("PlayerCombatController: Light Attack проигнорирован — игрок безоружен");
                return;
            }

            if (!TrySpendStamina(lightAttackData.staminaCost))
            {
                Debug.Log("PlayerCombatController: недостаточно stamina для Light Attack");
                return;
            }

            stateMachine.ChangeState(_attackState);
        }

        private void ReturnToFree()
        {
            stateMachine.ChangeState(_freeState);
        }

        /// <summary>Единственное место, где игрок тратит стамину на действие: базовая цена × множитель нагрузки экипировки.</summary>
        private bool TrySpendStamina(float baseCost)
        {
            return stamina == null || stamina.TryConsume(baseCost);
        }

        /// <summary>Оборвать всё, что игрок делает сейчас (смерть, оглушение).</summary>
        public void CancelAllActions()
        {
            _attackButtonHeld = false;
            _chargeStarted = false;

            weaponAttackController.CancelAttack(); // если шла атака — сам вернёт во Free
            if (stateMachine.CurrentState != _freeState)
            {
                stateMachine.ChangeState(_freeState); // Cast / Block / Dodge / Parry / Charge
            }
        }
        /// <summary>
        /// Оглушение игрока: обрывает атаку, заряд, каст, блок и парирование, затем держит
        /// StaggerState duration секунд. false — игрок в отскоке (Dodge), оглушение игнорируется.
        /// </summary>
        public bool TryStagger(float duration)
        {
            if (stateMachine.CurrentState == _dodgeState) return false;

            CancelAllActions();
            _staggerState.SetDuration(duration);
            stateMachine.ChangeState(_staggerState);
            return true;
        }
    }
}