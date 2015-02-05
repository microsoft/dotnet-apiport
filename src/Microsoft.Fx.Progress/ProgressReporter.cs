// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	internal sealed class ProgressReporter : IProgressReporter
	{
		private CancellationTokenSource _cancellationTokenSource;

		private bool _isRunning;
		private CancellationToken _cancellationToken;
		private string _task;
		private string _details;
		private float _percentageComplete;
		private bool _isIndeterminate;
		private TimeSpan _remainingTime;

		public IProgressMonitor CreateMonitor(CancellationTokenSource cancellationTokenSource)
		{
			return CreateMonitor(cancellationTokenSource, cancellationTokenSource.Token);
		}

		public IProgressMonitor CreateMonitor(CancellationToken cancellationToken)
		{
			return CreateMonitor(null, cancellationToken);
		}

		private IProgressMonitor CreateMonitor(CancellationTokenSource cancellationTokenSource, CancellationToken cancellationToken)
		{
			_cancellationTokenSource = cancellationTokenSource;
			_cancellationToken = cancellationToken;
			return new ReportingProgressMonitor(this);
		}

		public bool IsRunning
		{
			get { return _isRunning; }
			private set
			{
				if (_isRunning != value)
				{
					_isRunning = value;
					OnIsRunningChanged(EventArgs.Empty);
				}
			}
		}

		public CancellationToken CancellationToken
		{
			get { return _cancellationToken; }
		}

		public bool CanCancel
		{
			get { return _cancellationTokenSource != null && !_cancellationToken.IsCancellationRequested; }
		}

		public string Task
		{
			get { return _task; }
			set
			{
				if (_task != value)
				{
					_task = value;
					OnTaskChanged(EventArgs.Empty);
				}
			}
		}

		public string Details
		{
			get { return _details; }
			set
			{
				if (_details != value)
				{
					_details = value;
					OnDetailsChanged(EventArgs.Empty);
				}
			}
		}

		public float PercentageComplete
		{
			get { return _percentageComplete; }
			set
			{
				if (_percentageComplete != value)
				{
					_percentageComplete = value;
					OnPercentageCompleteChanged(EventArgs.Empty);
				}
			}
		}

		public bool IsIndeterminate
		{
			get { return _isIndeterminate; }
			set
			{
				if (_isIndeterminate != value)
				{
					_isIndeterminate = value;
					OnIsIndeterminateChanged(EventArgs.Empty);
				}
			}
		}

		public TimeSpan RemainingTime
		{
			get { return _remainingTime; }
			set
			{
				if (_remainingTime != value)
				{
					_remainingTime = value;
					OnRemainingTimeChanged(EventArgs.Empty);
				}
			}
		}

		public void Cancel()
		{
			if (!CanCancel)
				throw new NotSupportedException();

			_cancellationTokenSource.Cancel();
		}

		public void Start()
		{
			IsRunning = true;
			PercentageComplete = 0.0f;
			IsIndeterminate = true;
		}

		public void Finish()
		{
			IsRunning = false;
		}

		private void OnIsRunningChanged(EventArgs e)
		{
			var handler = IsRunningChanged;
			if (handler != null)
				handler(this, e);
		}

		private void OnTaskChanged(EventArgs e)
		{
			var handler = TaskChanged;
			if (handler != null)
				handler(this, e);
		}

		private void OnDetailsChanged(EventArgs e)
		{
			var handler = DetailsChanged;
			if (handler != null)
				handler(this, e);
		}

		private void OnPercentageCompleteChanged(EventArgs e)
		{
			var handler = PercentageCompleteChanged;
			if (handler != null)
				handler(this, e);
		}

		private void OnIsIndeterminateChanged(EventArgs e)
		{
			var handler = IsIndeterminateChanged;
			if (handler != null)
				handler(this, e);
		}

		private void OnRemainingTimeChanged(EventArgs e)
		{
			var handler = RemainingTimeChanged;
			if (handler != null)
				handler(this, e);
		}

		public event EventHandler IsRunningChanged;

		public event EventHandler TaskChanged;

		public event EventHandler DetailsChanged;

		public event EventHandler PercentageCompleteChanged;

		public event EventHandler IsIndeterminateChanged;

		public event EventHandler RemainingTimeChanged;
	}
}
