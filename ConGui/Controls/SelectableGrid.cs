using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.UserDefined;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConGui.Controls {
    public class SelectableGrid : SimpleControl, IInputListener {
        private int Cols;
        private int Rows;
        private int ColWidth;
        private int RowHeigth;

        private Grid grid;
        private List<(Background bckgnd, object context,  Action<object> callback)> Cells = new List<(Background, object, Action<object>)>();
        private int filledCellIdx = -1;
        private int? selectedCellIdx = null;
        private bool HorizontalFill = false;

        public Color DefaultBackgrndCol { get; set; } = new Color(0, 0, 0);
        public Color SelectedBackgrndCol { get; set; } = new Color(100, 0, 0);


        public SelectableGrid(int cols, int rows, int colWidth = 10, int rowHeigth = 1, bool horizontalFill = false) {
            this.Cols = cols;
            this.Rows = rows;
            this.ColWidth = colWidth;
            this.RowHeigth = rowHeigth;
            this.HorizontalFill = horizontalFill;

             grid = new Grid() {
                 Columns = Enumerable.Repeat(new Grid.ColumnDefinition(ColWidth), Cols).ToArray(),
                 Rows = Enumerable.Repeat(new Grid.RowDefinition(RowHeigth), Rows).ToArray()
             };
            Content = grid;
        }


        public void Add(IControl control, object context, Action<object> clicked) {
            filledCellIdx++;
            var cell = new Background {
                Content = control,
                Color = DefaultBackgrndCol
            };

           Cells.Add(new(cell, context, clicked));
            int x = 0, y = 0;
            if (HorizontalFill) {
                x = filledCellIdx % Cols;
                y = filledCellIdx / Cols;
            } else {
                x = filledCellIdx / Rows;
                y = filledCellIdx % Rows;
            }
            if ((x < Cols) && (y < Rows)) {
                grid.AddChild(x, y, cell);
            } 
            if (filledCellIdx == 0) {
                selectedCellIdx = 0;
                Cells[filledCellIdx].bckgnd.Color = SelectedBackgrndCol;
            }
        }

        public void OnInput(InputEvent inputEvent) {
            int? oldSelected = selectedCellIdx;

            if ((inputEvent.Key.Key == ConsoleKey.Enter)) {
                Cells[selectedCellIdx ?? 0].callback(Cells[selectedCellIdx ?? 0].context);
            } else if ((inputEvent.Key.Key == ConsoleKey.LeftArrow)) {
                if (selectedCellIdx - Rows >= 0) {
                    selectedCellIdx -= Rows;
                }
            } else if ((inputEvent.Key.Key == ConsoleKey.RightArrow)) {
                if (selectedCellIdx + Rows <= filledCellIdx) {
                    selectedCellIdx += Rows;
                }
            } else if ((inputEvent.Key.Key == ConsoleKey.UpArrow)) {
                if (selectedCellIdx - 1 >= 0) {
                    selectedCellIdx -= 1;
                }
            } else if ((inputEvent.Key.Key == ConsoleKey.DownArrow)) {
                if (selectedCellIdx + 1 <= filledCellIdx) {
                    selectedCellIdx += 1;
                }
            }

            if (selectedCellIdx != oldSelected) {
                if (selectedCellIdx != null) {
                    Cells[selectedCellIdx ?? 0].bckgnd.Color = SelectedBackgrndCol;
                }
                if (oldSelected != null) {
                    Cells[oldSelected ?? 0].bckgnd.Color = DefaultBackgrndCol;
                }
            }
        }
    }
}
