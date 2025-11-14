using Microsoft.Xna.Framework.Input;

namespace MonoGame.Extended.Input
{
    public class GamePadStateExtended
    {
        private GamePadState _currentGamePadState;
        private GamePadState _previousGamePadState;

        public GamePadStateExtended(GamePadState currentGamePadState, GamePadState previousGamePadState)
        {
            _currentGamePadState = currentGamePadState;
            _previousGamePadState = previousGamePadState;
        }

        public bool IsKeyDown(Buttons button) => _currentGamePadState.IsButtonDown(button);

        public bool IsKeyUp(Buttons button) => _currentGamePadState.IsButtonUp((button));


        public bool WasKeyReleased(Buttons button) => _previousGamePadState.IsButtonDown(button) && _currentGamePadState.IsButtonUp(button);

        public bool WasKeyPressed(Buttons button) => _previousGamePadState.IsButtonUp(button) && _currentGamePadState.IsButtonDown(button);
    }
}