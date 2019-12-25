using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public delegate void WorkerJobCompleted<T, U>(WorkerJob<T, U> job);

    public class WorkerJob<T, U>
    {
        public WorkerJob(ProgressDialog employer, T argument = default)
        {
            this.employer = employer;
            this.argument = argument;
        }

        public ProgressDialog employer;
        public T argument;
        public List<Exception> exceptions = new List<Exception>();
        public U result;

        public void Report(int percentage)
            => employer?.ReportProgress(percentage);
        public void Report(int percentage, string text, string description)
            => employer?.ReportProgress(percentage, text, description);
    }
}
