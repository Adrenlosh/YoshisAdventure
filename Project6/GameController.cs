using GameLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Project6;

/// <summary>
/// Provides a game-specific input abstraction that maps physical inputs
/// to game actions, bridging our input system with game-specific functionality.
/// </summary>
public static class GameController
{
    private static KeyboardInfo s_keyboard => GameMain.Input.Keyboard;
    private static GamePadInfo s_gamePad => GameMain.Input.GamePads[(int)PlayerIndex.One];

    /// <summary>
    /// Returns true if the player has triggered the "move up" action.
    /// </summary>
    public static bool MoveUp()
    {
        return s_keyboard.IsKeyDown(Keys.Up) ||
               s_keyboard.IsKeyDown(Keys.W) ||
               s_gamePad.IsButtonDown(Buttons.DPadUp) ||
               s_gamePad.IsButtonDown(Buttons.LeftThumbstickUp);
    }

    /// <summary>
    /// Returns true if the player has triggered the "move down" action.
    /// </summary>
    public static bool MoveDown()
    {
        return s_keyboard.IsKeyDown(Keys.Down) ||
               s_keyboard.IsKeyDown(Keys.S) ||
               s_gamePad.IsButtonDown(Buttons.DPadDown) ||
               s_gamePad.IsButtonDown(Buttons.LeftThumbstickDown);
    }

    /// <summary>
    /// Returns true if the player has triggered the "move left" action.
    /// </summary>
    public static bool MoveLeft()
    {
        return s_keyboard.IsKeyDown(Keys.Left) ||
               s_keyboard.IsKeyDown(Keys.A) ||
               s_gamePad.IsButtonDown(Buttons.DPadLeft) ||
               s_gamePad.IsButtonDown(Buttons.LeftThumbstickLeft);
    }

    /// <summary>
    /// Returns true if the player has triggered the "move right" action.
    /// </summary>
    public static bool MoveRight()
    {
        return s_keyboard.IsKeyDown(Keys.Right) ||
               s_keyboard.IsKeyDown(Keys.D) ||
               s_gamePad.IsButtonDown(Buttons.DPadRight) ||
               s_gamePad.IsButtonDown(Buttons.LeftThumbstickRight);
    }

    public static bool JumpPressed()
    {
        return s_keyboard.WasKeyJustPressed(Keys.L) ||
               s_gamePad.WasButtonJustPressed(Buttons.A);
    }

    public static bool JumpHeld()
    {
        return s_keyboard.IsKeyDown(Keys.L) ||
               s_gamePad.IsButtonDown(Buttons.A);
    }

    public static bool ActionPressed()
    {
        return s_keyboard.WasKeyJustPressed(Keys.K) ||
               s_gamePad.WasButtonJustPressed(Buttons.B);
    }

    public static bool ActionHeld()
    {
        return s_keyboard.IsKeyDown(Keys.K) ||
               s_gamePad.IsButtonDown(Buttons.B);
    }


    /// <summary>
    /// Returns true if the player has triggered the "pause" action.
    /// </summary>
    public static bool Pause()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Escape) ||
               s_gamePad.WasButtonJustPressed(Buttons.Start);
    }
}