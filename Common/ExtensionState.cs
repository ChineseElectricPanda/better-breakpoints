﻿using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBreakpoints.Common
{
    class ExtensionState
    {
        public static ExtensionState g_extensionState
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("ExtensionState cannot be accessed before it's initialized.");
                }

                return _instance;
            }
        }

        private static ExtensionState _instance = null;

        private DebuggerEvents _debuggerEvents;

        public DTE2 dte
        {
            get;
        }

        public Dictionary<string, Dictionary<int, BreakpointInfo>> breakpoints = new Dictionary<string, Dictionary<int, BreakpointInfo>>();


        public static void Initialize(DTE2 dte)
        {
            if (_instance != null)
            {
                throw new Exception("ExtensionState cannot be initialized more than once.");
            }

            _instance = new ExtensionState(dte);
        }

        private ExtensionState(DTE2 dte)
        {
            this.dte = dte;
            _debuggerEvents = dte.Events.DebuggerEvents;
            _debuggerEvents.OnEnterBreakMode += OnEnterBreakMode;
            _debuggerEvents.OnEnterRunMode += OnEnterRunMode;
            _debuggerEvents.OnEnterDesignMode += OnEnterDesignMode;
        }

        private void ForEachBreakpoint(Action<BreakpointInfo> func)
        {
            foreach (var fileBreakpoints in breakpoints.Values)
            {
                foreach (BreakpointInfo bpInfo in fileBreakpoints.Values)
                {
                    func(bpInfo);
                }
            }
        }

        private void DisableAllTriggeredBreakpoints()
        {
            ForEachBreakpoint(bpInfo =>
            {
                if ((bpInfo.mode == BreakpointMode.NotTriggered) || (bpInfo.mode == BreakpointMode.Triggered))
                {
                    bpInfo.DisableNativeBreakpoint();
                    bpInfo.mode = BreakpointMode.NotTriggered;
                }
            });
        }
        private void OnEnterDesignMode(dbgEventReason Reason)
        {
            // When we're done running, disable all triggered breakpoints
            DisableAllTriggeredBreakpoints();
        }

        private void OnEnterRunMode(dbgEventReason reason)
        {
            if (reason == dbgEventReason.dbgEventReasonLaunchProgram)
            {
                // When the program is run, disable all triggered breakpoints
                DisableAllTriggeredBreakpoints();
            }
        }

        private void OnEnterBreakMode(dbgEventReason reason, ref dbgExecutionAction executionAction)
        {
            if (reason == dbgEventReason.dbgEventReasonBreakpoint)
            {
                string filePath = dte.Debugger.BreakpointLastHit.File;
                int lineNumber = dte.Debugger.BreakpointLastHit.FileLine;

                BreakpointInfo bpInfo = bpInfo = GetBreakpoint(filePath, lineNumber);

                if (bpInfo != null)
                {
                    // If we hit a trigger, enable all triggered breakpoints of the same color
                    if ((bpInfo.mode == BreakpointMode.TriggerAndBreak) || (bpInfo.mode == BreakpointMode.TriggerAndContinue))
                    {
                        ForEachBreakpoint(bpInfo2 =>
                        {
                            if ((bpInfo.color == bpInfo2.color) && (bpInfo2.mode == BreakpointMode.NotTriggered))
                            {
                                bpInfo2.EnableNativeBreakpoint();
                                bpInfo2.mode = BreakpointMode.Triggered;
                            }
                        });
                    }

                    if (bpInfo.mode == BreakpointMode.TriggerAndContinue)
                    {
                        executionAction = dbgExecutionAction.dbgExecutionActionGo;
                    }
                }
            }
        }

        public List<BreakpointInfo> GetAllbreakpointsInFile(string filePath)
        {
            if (breakpoints.ContainsKey(filePath))
            {
                return breakpoints[filePath].Values.ToList();
            }
            else
            {
                return new List<BreakpointInfo>();
            }
        }

        /// <summary>
        /// Creates a breakpoint with the specified parameters and returns it. If it already exists, return the existing instance.
        /// </summary>
        public BreakpointInfo CreateBreakpoint(string filePath, int lineNumber)
        {
            if (breakpoints.ContainsKey(filePath))
            {
                if (breakpoints[filePath].ContainsKey(lineNumber))
                {
                    return breakpoints[filePath][lineNumber];
                }
                else
                {
                    breakpoints[filePath][lineNumber] = new BreakpointInfo(filePath, lineNumber);
                    breakpoints[filePath][lineNumber].CreateNativeBreakpoint(dte);
                }
            }
            else
            {
                breakpoints[filePath] = new Dictionary<int, BreakpointInfo>();
                breakpoints[filePath][lineNumber] = new BreakpointInfo(filePath, lineNumber);
                breakpoints[filePath][lineNumber].CreateNativeBreakpoint(dte);
            }

            return breakpoints[filePath][lineNumber];
        }

        public BreakpointInfo GetBreakpoint(string filePath, int lineNumber)
        {
            if (breakpoints.ContainsKey(filePath) && breakpoints[filePath].ContainsKey(lineNumber))
            {
                return breakpoints[filePath][lineNumber];
            }
            else
            {
                return null;
            }
        }

        public void RemoveBreakpoint(string filePath, int lineNumber)
        {
            if (breakpoints.ContainsKey(filePath) && breakpoints[filePath].ContainsKey(lineNumber))
            {
                breakpoints[filePath][lineNumber].RemoveNativeBreakpoint();
                breakpoints[filePath].Remove(lineNumber);
            }
        }
    }
}
