using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Input;

namespace Project6;

/// <summary>
/// Provides a game-specific input abstraction that maps physical inputs
/// to game actions, bridging our input system with game-specific functionality.
/// </summary>
public static class GameController
{
    private static KeyboardInfo s_keyboard => Core.Input.Keyboard;
    private static GamePadInfo s_gamePad => Core.Input.GamePads[(int)PlayerIndex.One];

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

        
    public static bool APressed()
    {
        return s_keyboard.WasKeyJustPressed(Keys.L) ||
               s_gamePad.WasButtonJustPressed(Buttons.A);
    }
    public static bool AReleased()
    {
        return s_keyboard.WasKeyJustReleased(Keys.L) ||
               s_gamePad.WasButtonJustReleased(Buttons.A);
    }


    public static bool AHeld()
    {
        return s_keyboard.IsKeyDown(Keys.L) ||
               s_gamePad.IsButtonDown(Buttons.A);
    }

    public static bool BPressed()
    {
        return s_keyboard.WasKeyJustPressed(Keys.K) ||
               s_gamePad.WasButtonJustPressed(Buttons.B);
    }


    /// <summary>
    /// Returns true if the player has triggered the "pause" action.
    /// </summary>
    public static bool Pause()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Escape) ||
               s_gamePad.WasButtonJustPressed(Buttons.Start);
    }

    /// <summary>
    /// Returns true if the player has triggered the "action" button,
    /// typically used for menu confirmation.
    /// </summary>
    public static bool Action()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Enter) ||
               s_gamePad.WasButtonJustPressed(Buttons.A);
    }
}