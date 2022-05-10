﻿using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.UserDefined;

public class LogPanel : SimpleControl, IInputListener {
	private readonly VerticalStackPanel _stackPanel;
	private readonly VerticalScrollPanel _scrollPanel;

	public LogPanel()  {
		_stackPanel = new VerticalStackPanel();
		_scrollPanel = new VerticalScrollPanel() {
			Content = _stackPanel,
			Top = 0

		};
		Content = _scrollPanel;
	}


	public void Add(string message) {
		Monitor.Enter(this);				// This has to be Thread Save! its used by all Logger instances!
		_stackPanel.Add(new WrapPanel {
			Content = new HorizontalStackPanel {
				Children = new[]
				{
						new TextBlock {Text = $"[{DateTime.Now.ToLongTimeString()}] ", Color = new Color(200, 20, 20)},
						new TextBlock {Text = message}
					}
			}
		});
		_scrollPanel.Top = _stackPanel.Children.Count() - this.Size.Height;
		Monitor.Exit(this);
	}

    public void OnInput(InputEvent inputEvent) {
		if (inputEvent.Key.Key == ConsoleKey.DownArrow) {
			_scrollPanel.Top += 1;
		} else if (inputEvent.Key.Key == ConsoleKey.UpArrow) {
			_scrollPanel.Top -= 1;
		} else if (inputEvent.Key.Key == ConsoleKey.PageDown) {
			_scrollPanel.Top += this.Size.Height;
		} else if (inputEvent.Key.Key == ConsoleKey.PageUp) {
			_scrollPanel.Top -= this.Size.Height;
		}
    }
}

