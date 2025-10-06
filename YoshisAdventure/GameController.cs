using GameLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace YoshisAdventure;

/// <summary>
/// Provides a game-specific input abstraction that maps physical inputs
/// to game actions, bridging our input system with game-specific functionality.
/// </summary>
public static class GameController
{
    private static KeyboardInfo Keyboard => GameMain.Input.Keyboard;

    private static GamePadInfo GamePad => GameMain.Input.GamePads[(int)PlayerIndex.One];

    /// <summary>
    /// Returns true if the player has triggered the "move up" action.
    /// </summary>
    public static bool MoveUp()
    {
        return Keyboard.IsKeyDown(Keys.Up) ||
               Keyboard.IsKeyDown(Keys.W) ||
               GamePad.IsButtonDown(Buttons.DPadUp) ||
               GamePad.IsButtonDown(Buttons.LeftThumbstickUp);
    }

    /// <summary>
    /// Returns true if the player has triggered the "move down" action.
    /// </summary>
    public static bool MoveDown()
    {
        return Keyboard.IsKeyDown(Keys.Down) ||
               Keyboard.IsKeyDown(Keys.S) ||
               GamePad.IsButtonDown(Buttons.DPadDown) ||
               GamePad.IsButtonDown(Buttons.LeftThumbstickDown);
    }

    /// <summary>
    /// Returns true if the player has triggered the "move left" action.
    /// </summary>
    public static bool MoveLeft()
    {
        return Keyboard.IsKeyDown(Keys.Left) ||
               Keyboard.IsKeyDown(Keys.A) ||
               GamePad.IsButtonDown(Buttons.DPadLeft) ||
               GamePad.IsButtonDown(Buttons.LeftThumbstickLeft);
    }

    /// <summary>
    /// Returns true if the player has triggered the "move right" action.
    /// </summary>
    public static bool MoveRight()
    {
        return Keyboard.IsKeyDown(Keys.Right) ||
               Keyboard.IsKeyDown(Keys.D) ||
               GamePad.IsButtonDown(Buttons.DPadRight) ||
               GamePad.IsButtonDown(Buttons.LeftThumbstickRight);
    }

    public static bool JumpPressed()
    {
        return Keyboard.WasKeyJustPressed(Keys.L) ||
               GamePad.WasButtonJustPressed(Buttons.A);
    }

    public static bool JumpHeld()
    {
        return Keyboard.IsKeyDown(Keys.L) ||
               GamePad.IsButtonDown(Buttons.A);
    }

    public static bool ActionPressed()
    {
        return Keyboard.WasKeyJustPressed(Keys.K) ||
               GamePad.WasButtonJustPressed(Buttons.B);
    }

    public static bool ActionHeld()
    {
        return Keyboard.IsKeyDown(Keys.K) ||
               GamePad.IsButtonDown(Buttons.B);
    }

    public static bool AttackPressed()
    {
        return Keyboard.WasKeyJustPressed(Keys.O) ||
            GamePad.WasButtonJustPressed(Buttons.X);
    }

    public static bool StartPressed()
    {
        return Keyboard.WasKeyJustPressed(Keys.B) ||
            GamePad.WasButtonJustPressed(Buttons.Start);
    }

    public static bool BackPressed()
    {
        return Keyboard.WasKeyJustPressed(Keys.Escape) ||
            GamePad.WasButtonJustPressed(Buttons.Back);
    }
}