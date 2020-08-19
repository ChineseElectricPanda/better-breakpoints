using EnvDTE;
using EnvDTE100;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace BetterBreakpoints.Common
{
    public enum BreakpointMode
    {
        TriggerAndBreak,
        TriggerAndContinue,
        NotTriggered,
        Triggered,
    }

    public enum BreakpointColor
    {
        Green,
        Blue,
        Orange,
    }

    public class BreakpointInfo : IComparable<BreakpointInfo>
    {
        public delegate void BreakpointChangedHandler(BreakpointInfo bpInfo);
        public event BreakpointChangedHandler OnBreakpointChanged;

        public string filePath;
        public int lineNumber;
        public BreakpointMode mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                OnBreakpointChanged?.Invoke(this);
            }
        }
        public BreakpointColor color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnBreakpointChanged?.Invoke(this);
            }
        }

        private Breakpoint _nativeBreakpoint = null;
        private BreakpointMode _mode;
        private BreakpointColor _color;

        public BreakpointInfo(string filePath, int lineNumber)
        {
            this.filePath = filePath;
            this.lineNumber = lineNumber;
            this.mode = BreakpointMode.TriggerAndBreak;
            this.color = BreakpointColor.Green;
        }

        public void EnableNativeBreakpoint()
        {
            if (_nativeBreakpoint != null)
            {
                _nativeBreakpoint.Enabled = true;
            }
        }

        public void DisableNativeBreakpoint()
        {
            if (_nativeBreakpoint != null)
            {
                _nativeBreakpoint.Enabled = false;
            }
        }

        public bool AttachNativeBreakpoint(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if(_nativeBreakpoint == null)
            {
                foreach(Breakpoint bp in ((Debugger5)dte.Debugger).Breakpoints)
                {
                    if ((bp.File == filePath) && (bp.FileLine == lineNumber))
                    {
                        _nativeBreakpoint = bp;
                        return true;
                    }
                }
            }

            return false;
        }

        public void CreateNativeBreakpoint(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_nativeBreakpoint == null)
            {
                if (!AttachNativeBreakpoint(dte))
                {
                    Breakpoints breakpoints = ((Debugger5)dte.Debugger).Breakpoints.Add(File: filePath, Line: lineNumber);
                    _nativeBreakpoint = breakpoints.Item(1);
                }
            }
        }

        public void RemoveNativeBreakpoint()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                _nativeBreakpoint?.Delete();
            }
            catch (COMException) { }

            _nativeBreakpoint = null;
        }

        public SolidColorBrush GetBrush()
        {
            switch (color)
            {
                case BreakpointColor.Green: return Brushes.Green;
                case BreakpointColor.Blue: return Brushes.Blue;
                case BreakpointColor.Orange: return Brushes.Orange;
                default: return Brushes.Transparent;
            }
        }
        public int CompareTo(BreakpointInfo other)
        {
            if (!this.filePath.Equals(other.filePath))
            {
                return this.filePath.CompareTo(other.filePath);
            }
            else if (this.lineNumber != other.lineNumber)
            {
                return (this.lineNumber - other.lineNumber);
            }
            else
            {
                return 0;
            }
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                BreakpointInfo other = (BreakpointInfo)obj;
                return (other.filePath.Equals(this.filePath) && (other.lineNumber == this.lineNumber));
            }
        }

        public override int GetHashCode()
        {
            int hashCode = -1357422131;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(filePath);
            hashCode = hashCode * -1521134295 + lineNumber.GetHashCode();
            return hashCode;
        }
    }
}
