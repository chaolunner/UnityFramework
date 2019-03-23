using System.Collections.Generic;
using UnityEngine;
using System;

namespace UniEasy.Console
{
    public class ConsoleInputHistory
    {
        private int MaxCapacity;
        private int CurrentInput;
        private bool IsNavigating;
        private List<string> InputHistory;

        public ConsoleInputHistory(int maxCapacity)
        {
            InputHistory = new List<string>(maxCapacity);
            MaxCapacity = maxCapacity;
        }

        public string Navigate(bool up)
        {
            bool down = !up;

            if (!IsNavigating)
            {
                IsNavigating = (up && InputHistory.Count > 0) || (down && CurrentInput > 0);
            }
            else if (up)
            {
                CurrentInput++;
            }

            if (down)
            {
                CurrentInput--;
            }

            CurrentInput = Mathf.Clamp(CurrentInput, 0, InputHistory.Count - 1);

            if (IsNavigating)
            {
                return InputHistory[CurrentInput];
            }
            else
            {
                return "";
            }
        }

        public void AddNewInputEntry(string input)
        {
            IsNavigating = false;

            if (InputHistory.Count > 0 && input.Equals(InputHistory[0], StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (InputHistory.Count == MaxCapacity)
            {
                InputHistory.RemoveAt(MaxCapacity - 1);
            }

            InputHistory.Insert(0, input);

            if (CurrentInput == MaxCapacity - 1)
            {
                CurrentInput = 0;
            }
            else
            {
                CurrentInput = Mathf.Clamp(++CurrentInput, 0, InputHistory.Count - 1);
            }

            if (!input.Equals(InputHistory[CurrentInput], StringComparison.OrdinalIgnoreCase))
            {
                CurrentInput = 0;
            }
        }

        public void Clear()
        {
            InputHistory.Clear();
            CurrentInput = 0;
            IsNavigating = false;
        }

        public string PreviousCommand(int index)
        {
            IsNavigating = true;
            CurrentInput = index;
            return index >= 0 && index < InputHistory.Count ? InputHistory[index] : null;
        }
    }
}
