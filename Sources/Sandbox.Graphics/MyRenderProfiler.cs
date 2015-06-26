﻿#region Using

using System.Diagnostics;
using VRage.Input;
using VRageRender;

#endregion


namespace Sandbox
{
    public class MyRenderProfiler
    {
        [Conditional(VRage.ProfilerShort.Symbol)]
        public static void HandleInput()
        {
            RenderProfilerCommand? command = null;
            int index = 0;
            bool sleep = false;

            if (MyInput.Static.IsAnyAltKeyPressed())
            {
                for (int i = 0; i <= 9; i++)
                {
                    var key = (MyKeys)((int)MyKeys.NumPad0 + i);
                    if (MyInput.Static.IsNewKeyPressed(key))
                    {
                        index = i;
                        command = RenderProfilerCommand.JumpToLevel;
                    }
                }

                if (MyInput.Static.IsAnyCtrlKeyPressed() && !MyInput.Static.IsKeyPress(MyKeys.Space))
                {
                    index += 10;
                    sleep = true;
                }

                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsKeyPress(MyKeys.Space))
                {
                    index += 20;
                    sleep = true;
                }

                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad0) && index == 0)
                {
                    command = RenderProfilerCommand.Enable;
                    sleep = true;
                }

                if (MyInput.Static.IsAnyCtrlKeyPressed())
                {
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                    {
                        command = RenderProfilerCommand.IncreaseLocalArea;
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                    {
                        command = RenderProfilerCommand.DecreaseLocalArea;
                    }
                }
                else
                {
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                    {
                        command = RenderProfilerCommand.NextThread;
                        sleep = true;
                    }

                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                    {
                        command = RenderProfilerCommand.PreviousThread;
                        sleep = true;
                    }
                }

                if (MyInput.Static.IsNewKeyPressed(MyKeys.Decimal))
                {
                    command = RenderProfilerCommand.FindMaxChild;
                }

                if (MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
                {
                    command = RenderProfilerCommand.Pause;
                    sleep = true;
                }

                if (MyInput.Static.IsAnyCtrlKeyPressed()) // Precision mode
                {
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.PageDown))
                        command = RenderProfilerCommand.PreviousFrame;

                    if (MyInput.Static.IsNewKeyPressed(MyKeys.PageUp))
                        command = RenderProfilerCommand.NextFrame;
                }
                else
                {
                    if (MyInput.Static.IsKeyPress(MyKeys.PageDown))
                        command = RenderProfilerCommand.PreviousFrame;

                    if (MyInput.Static.IsKeyPress(MyKeys.PageUp))
                        command = RenderProfilerCommand.NextFrame;
                }

                if (MyInput.Static.IsNewKeyPressed(MyKeys.Home))
                    command = RenderProfilerCommand.IncreaseRange;
                if (MyInput.Static.IsNewKeyPressed(MyKeys.End))
                    command = RenderProfilerCommand.DecreaseRange;

                if (MyInput.Static.IsAnyCtrlKeyPressed()) // Precision mode
                {
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Multiply))
                        command = RenderProfilerCommand.IncreaseLevel;

                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Divide))
                        command = RenderProfilerCommand.DecreaseLevel;
                }
                else
                {
                    if (MyInput.Static.IsKeyPress(MyKeys.Multiply))
                        command = RenderProfilerCommand.IncreaseLevel;

                    if (MyInput.Static.IsKeyPress(MyKeys.Divide))
                        command = RenderProfilerCommand.DecreaseLevel;
                }
            }

            if (command.HasValue)
            {
                VRageRender.MyRenderProxy.RenderProfilerInput(command.Value, index);

                if (sleep)
                {
                    // TODO: OP! Why is there sleep?
                    //System.Threading.Thread.Sleep(100);
                }
            }
        }
    }
}
