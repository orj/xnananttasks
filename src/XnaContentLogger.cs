/* Copyright (c) Oliver Jones
 * This work is licensed under a Creative Commons Attribution 3.0 License.
 * http://creativecommons.org/licenses/by/3.0/
 *
 * Created by: Oliver Jones
 * Created: Sunday, 1 April 2007
 * SVN: $Id$
 */

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NAnt.Core;
using BuildEventArgs=Microsoft.Build.Framework.BuildEventArgs;
using Task=NAnt.Core.Task;

namespace NAnt.Examples.Tasks
{
    /// <summary>An adapter from an MSBuild ILogger to NAnt logger.</summary>
    public class XnaContentLogger : Logger
    {
        private Task m_task;

        public XnaContentLogger(Task task)
        {
            m_task = task;
        }

        public override void Initialize(IEventSource eventSource)
        {
            eventSource.BuildFinished += eventSource_InfoEventRaised;
            eventSource.BuildStarted += eventSource_InfoEventRaised;
            eventSource.ProjectFinished += eventSource_InfoEventRaised;
            eventSource.ProjectStarted += eventSource_InfoEventRaised;

            eventSource.ErrorRaised += eventSource_ErrorRaised;
            eventSource.WarningRaised += eventSource_WarningRaised;
            
            eventSource.CustomEventRaised += eventSource_VerboseEventRaised;
            eventSource.TargetStarted += eventSource_VerboseEventRaised;
            eventSource.TargetFinished += eventSource_VerboseEventRaised;
            eventSource.MessageRaised += eventSource_VerboseEventRaised;
            eventSource.StatusEventRaised += eventSource_VerboseEventRaised;
            eventSource.TaskFinished += eventSource_VerboseEventRaised;
            eventSource.TaskStarted += eventSource_VerboseEventRaised;

        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            m_task.Log(Level.Warning, e.Message);
        }

        void eventSource_VerboseEventRaised(object sender, BuildEventArgs e)
        {
            m_task.Log(Level.Verbose, e.Message);
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            m_task.Log(Level.Error, e.Message);
        }

        void eventSource_InfoEventRaised(object sender, BuildEventArgs e)
        {
            m_task.Log(Level.Info, e.Message);
        }
    }
}