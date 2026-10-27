using System;
using UnityEngine;
using Game.Magic;

namespace Game.Player
{
    /// <summary>
    /// Каст заклинания активного кольца удержанием кнопки.
    ///   Charging — кнопка зажата, ничего не списывается. Отпустить раньше castTime = отмена без трат.
    ///     После castTime заряд готов и держится сколько угодно; при отпускании SpellCaster.TryRelease
    ///     выпускает эффекты, списывает стоимость и запускает кулдаун.
    ///   Recovery — после выпуска игрок ещё recoveryTime не может начать другое боевое действие.
    /// Длительности берутся у SpellCaster (GetCastTime/GetRecoveryTime): они учитывают характеристику CastSpeed.
    /// castTime = 0 (SpellData.IsInstant) — мгновенный каст: выпуск прямо в Enter(), без заряда.
    /// Charging прерываем (CanInterrupt): любое другое боевое действие уходит в другое состояние,
    /// Exit() отменяет заряд бесплатно. Ходить можно во всех фазах, замедления нет.
    /// Время идёт через Tick, без корутин.
    ///
    /// TODO Phase 8: прерывание заряда полученным ударом не реализовано —
    /// DamageReceiver.ReceiveStagger пока пустой. В Phase 8 решить, прерывать ли заряд
    /// только сильными ударами (по порогу staggerPower), чтобы сложность оставалась
    /// между Lunacid и King's Field. Прерывать достаточно переходом в FreeState:
    /// Exit() сам отменит заряд без трат.
    /// </summary>
    public class CastState : IPlayerActionState
    {
        private enum CastPhase
        {
            Charging,
            Recovery,
            Done
        }

        private readonly SpellCaster _spellCaster;
        private readonly RingSpellController _ringSpellController;
        private readonly Action _onFinished;

        private SpellData _spell;
        private CastPhase _phase = CastPhase.Done;
        private float _elapsed;
        private bool _chargeReady;

        public CastState(SpellCaster spellCaster, RingSpellController ringSpellController, Action onFinished)
        {
            _spellCaster = spellCaster;
            _ringSpellController = ringSpellController;
            _onFinished = onFinished;
        }

        /// <summary>Игрок сейчас держит кнопку и заряд ещё не выпущен.</summary>
        public bool IsCharging => _phase == CastPhase.Charging && _spell != null;

        public void Enter()
        {
            _elapsed = 0f;
            _chargeReady = false;
            _spell = _ringSpellController.ActiveSpell;

            if (_spell == null || !_spellCaster.CanCast(_spell))
            {
                Abort(); // завершится в первом Tick
                return;
            }

            if (_spell.IsInstant)
            {
                if (_spellCaster.TryRelease(_spell))
                {
                    _phase = CastPhase.Recovery;
                }
                else
                {
                    Abort();
                }
                return;
            }

            _phase = CastPhase.Charging;
            _spellCaster.StartCharge(_spell);
        }

        public void Tick()
        {
            switch (_phase)
            {
                case CastPhase.Done:
                    _onFinished?.Invoke();
                    break;

                case CastPhase.Charging:
                    _elapsed += Time.deltaTime;
                    if (!_chargeReady && _elapsed >= _spellCaster.GetCastTime(_spell))
                    {
                        _chargeReady = true;
                        _spellCaster.ReportChargeReady(_spell);
                    }
                    break;

                case CastPhase.Recovery:
                    _elapsed += Time.deltaTime;
                    if (_elapsed >= _spellCaster.GetRecoveryTime(_spell))
                    {
                        _onFinished?.Invoke();
                    }
                    break;
            }
        }

        /// <summary>Кнопка каста отпущена. Вне фазы Charging игнорируется.</summary>
        public void NotifyReleased()
        {
            if (!IsCharging) return;

            // Каст срывается без трат, если: отпустили раньше castTime, за время заряда
            // сменилось активное кольцо, не хватило ресурса или эффекты не могут сработать.
            bool released = _chargeReady
                && _ringSpellController.ActiveSpell == _spell
                && _spellCaster.TryRelease(_spell);

            if (released)
            {
                _phase = CastPhase.Recovery;
                _elapsed = 0f;
            }
            else
            {
                _onFinished?.Invoke(); // Exit() отправит событие отмены заряда
            }
        }

        public void Exit()
        {
            if (IsCharging)
            {
                _spellCaster.CancelCharge(_spell);
            }

            _spell = null;
            _phase = CastPhase.Done;
        }

        /// <summary>Заряд можно прервать любым другим действием; recovery после выпуска — нельзя.</summary>
        public bool CanInterrupt() => _phase != CastPhase.Recovery;

        private void Abort()
        {
            _spell = null;
            _phase = CastPhase.Done;
        }
    }
}
