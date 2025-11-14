using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace YoshisAdventure.Systems;

public static class GameControllerSystem
{
    private static readonly PlayerIndex _playerIndex = PlayerIndex.One;

    public static void Update()
    {
        KeyboardExtended.Update();
        GamePadExtended.Update(_playerIndex);
    }

    private static KeyboardStateExtended GetStateKeyboard() => KeyboardExtended.GetState();

    private static GamePadStateExtended GetStatePad() => GamePadExtended.GetState(_playerIndex);

    public static bool MoveUp()
    {
        return GetStateKeyboard().IsKeyDown(Keys.Up) ||
               GetStateKeyboard().IsKeyDown(Keys.W) ||
               GetStatePad().IsKeyDown(Buttons.DPadUp) ||
               GetStatePad().IsKeyDown(Buttons.LeftThumbstickUp);
    }

    public static bool MoveDown()
    {
        return GetStateKeyboard().IsKeyDown(Keys.Down) ||
               GetStateKeyboard().IsKeyDown(Keys.S) ||
               GetStatePad().IsKeyDown(Buttons.DPadDown) ||
               GetStatePad().IsKeyDown(Buttons.LeftThumbstickDown);
    }

    public static bool MoveLeft()
    {
        return GetStateKeyboard().IsKeyDown(Keys.Left) ||
               GetStateKeyboard().IsKeyDown(Keys.A) ||
               GetStatePad().IsKeyDown(Buttons.DPadLeft) ||
               GetStatePad().IsKeyDown(Buttons.LeftThumbstickLeft);
    }

    public static bool MoveRight()
    {
        return GetStateKeyboard().IsKeyDown(Keys.Right) ||
               GetStateKeyboard().IsKeyDown(Keys.D) ||
               GetStatePad().IsKeyDown(Buttons.DPadRight) ||
               GetStatePad().IsKeyDown(Buttons.LeftThumbstickRight);
    }

    public static bool JumpPressed()
    {
        return GetStateKeyboard().WasKeyPressed(Keys.L) ||
               GetStatePad().WasKeyPressed(Buttons.A);
    }

    public static bool JumpHeld()
    {
        return GetStateKeyboard().IsKeyDown(Keys.L) ||
               GetStatePad().IsKeyDown(Buttons.A);
    }

    public static bool ActionPressed()
    {
        return GetStateKeyboard().WasKeyPressed(Keys.K) ||
               GetStatePad().WasKeyPressed(Buttons.B);
    }

    public static bool ActionHeld()
    {
        return GetStateKeyboard().IsKeyDown(Keys.K) ||
               GetStatePad().IsKeyDown(Buttons.B);
    }

    public static bool AttackPressed()
    {
        return GetStateKeyboard().WasKeyPressed(Keys.O) ||
            GetStatePad().WasKeyPressed(Buttons.X);
    }

    public static bool StartPressed()
    {
        return GetStateKeyboard().WasKeyPressed(Keys.B) ||
            GetStatePad().WasKeyPressed(Buttons.Start);
    }

    public static bool BackPressed()
    {
        return GetStateKeyboard().WasKeyPressed(Keys.Escape) ||
            GetStatePad().WasKeyPressed(Buttons.Back);
    }
}