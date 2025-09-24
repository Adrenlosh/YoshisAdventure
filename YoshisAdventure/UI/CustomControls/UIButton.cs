using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using System;

namespace YoshisAdventure.UI.CustomControls
{
    public class UIButton : Button
    {
        ContainerRuntime topLevelContainer = new ContainerRuntime();
        RectangleRuntime rectangleRumtime = new RectangleRuntime();
        TextRuntime textInstance = new TextRuntime();

        public int Alpha { 
            get => rectangleRumtime.Alpha;
            set => rectangleRumtime.Alpha = textInstance.Alpha = value;
        }

        public UIButton()
        {
            
            topLevelContainer.Height = 14f;
            topLevelContainer.HeightUnits = DimensionUnitType.Absolute;
            topLevelContainer.Width = 21f;
            topLevelContainer.WidthUnits = DimensionUnitType.RelativeToChildren;

            
            rectangleRumtime.Alpha = 0;
            rectangleRumtime.Red = 255;
            rectangleRumtime.Green = 255;
            rectangleRumtime.Blue = 0;
            rectangleRumtime.Dock(Gum.Wireframe.Dock.Fill);
            topLevelContainer.Children.Add(rectangleRumtime);
            
            textInstance.Name = "TextInstance";
            textInstance.Text = "Click Me";
            textInstance.Blue = 255;
            textInstance.Green = 255;
            textInstance.Red = 255;
            textInstance.UseCustomFont = true;
            textInstance.CustomFontFile = "Fonts/ZFull-GB.fnt";
            textInstance.FontScale = 1f;
            textInstance.Dock(Gum.Wireframe.Dock.Fill);
            textInstance.Width = 0;
            textInstance.WidthUnits = DimensionUnitType.RelativeToChildren;
            rectangleRumtime.Children.Add(textInstance);

            StateSaveCategory category = new StateSaveCategory();
            category.Name = Button.ButtonCategoryName;
            topLevelContainer.AddCategory(category);

            StateSave enabledState = new StateSave();
            enabledState.Name = FrameworkElement.EnabledStateName;
            enabledState.Apply = () =>
            {
                rectangleRumtime.Alpha = 0;
            };
            category.States.Add(enabledState);

            StateSave focusedState = new StateSave();
            focusedState.Name = FrameworkElement.FocusedStateName;
            focusedState.Apply = () =>
            {
                rectangleRumtime.Alpha = 255;
            };
            category.States.Add(focusedState);

            StateSave highlightedFocused = focusedState.Clone();
            highlightedFocused.Name = FrameworkElement.HighlightedFocusedStateName;
            category.States.Add(highlightedFocused);

            StateSave highlighted = enabledState.Clone();
            highlighted.Name = FrameworkElement.HighlightedStateName;
            category.States.Add(highlighted);

            KeyDown += HandleKeyDown;
            topLevelContainer.RollOn += HandleRollOn;
            Visual = topLevelContainer;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Left)
            {
                HandleTab(TabDirection.Up, loop: true);
            }
            if (e.Key == Keys.Right)
            {
                HandleTab(TabDirection.Down, loop: true);
            }
        }

        private void HandleRollOn(object sender, EventArgs e)
        {
            IsFocused = true;
        }
    }
}
