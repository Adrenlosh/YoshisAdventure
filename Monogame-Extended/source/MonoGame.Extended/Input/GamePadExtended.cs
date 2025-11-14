using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.Extended.Input
{
    public static class GamePadExtended
    {
        private static GamePadState _currentGamePadState;
        private static GamePadState _previousGamePadState;

        public static GamePadStateExtended GetState(PlayerIndex playerIndex)
        {
            return new GamePadStateExtended(_currentGamePadState, _previousGamePadState);
        }

        public static void Update(PlayerIndex playerIndex)
        {
            _previousGamePadState = _currentGamePadState;
            _currentGamePadState = GamePad.GetState(playerIndex);
        }
    }
}
