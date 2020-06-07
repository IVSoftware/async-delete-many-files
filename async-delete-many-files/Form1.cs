using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace async_delete_many_files
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            buttonDelete.Click += ButtonDelete_Click;
            buttonCancel.Click += ButtonCancel_Click;
            FileDeleted += TaskNotify_FileDeleted;
            _remaining = NUMBER_OF_FILES_TO_DELETE;
            textBoxRemaining.Text = NUMBER_OF_FILES_TO_DELETE.ToString();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
            MessageBox.Show("Cancelled");
        }

        const int NUMBER_OF_FILES_TO_DELETE = 100;
        int _remaining;

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            Task.Run(() => DeleteManyFiles());
            buttonCancel.Enabled = true;
        }
        CancellationTokenSource _cts = null;
        SemaphoreSlim ssBusy = new SemaphoreSlim(2);
        private void DeleteManyFiles()
        {
            try
            {
                ssBusy.Wait();
                switch (ssBusy.CurrentCount)
                {
                    case 1:
                        _cts = new CancellationTokenSource();
                        for (int i = 0; i < NUMBER_OF_FILES_TO_DELETE; i++)
                        {
                            bool cancelled = DeleteSingleFile(_cts.Token);
                            if(cancelled) break;
                        }
                        break;
                    case 0:
                        // A count of 0 indicates that the operation is already in progress.
                        MessageBox.Show("Deletion is already in progress");
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
            finally
            {
                ssBusy.Release();
            }
        }
        private bool DeleteSingleFile(CancellationToken ct)
        {
            if(ct.IsCancellationRequested)
            {
                return true;
            }
            // Simulate half a second to delete one file
            Task.Delay(500).Wait();
            // SIMULATE ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            // Decrement remaining count
            _remaining--;

            // Notify the UI thread
            FileDeleted?.Invoke(this, EventArgs.Empty);
            return false;
        }
        event EventHandler FileDeleted;

        private void TaskNotify_FileDeleted(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate 
            {
                textBoxRemaining.Text = _remaining.ToString(); 
            });
        }
    }
}
