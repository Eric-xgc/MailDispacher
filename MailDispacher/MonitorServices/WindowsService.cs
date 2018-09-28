﻿using MonitorServices.Properties;
using MonitorServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;

namespace MonitorServices
{
    public class WindowsService
    {
        private LogWriter _log = HostLogger.Get<WindowsService>();
        private Timer _timer;
        public void Start()
        {
            try
            {
                string messaggio = string.Format("Timer impostato a {0} minuti", Settings.Default.TimerPeriod);
                _timer = new Timer(TimerCallBack, null, 1L, Settings.Default.TimerPeriod * 60 * 1000);
                _log.Info("Servizio Monitor avviato");

            }
            catch (Exception ex)
            {
                _log.Error("Errore in fase di start del servizio", ex);

            }
        }

        public void Stop()
        {
            try
            {
                _timer.Dispose();
                _log.Info("Servizio Monitor fermato");
            }
            catch (Exception ex)
            {
                _log.Error("Errore in fase di stop del servizio", ex);
            }
        }

        private void TimerCallBack(Object stateInfo)
        {
            try
            {
                MagazzinoMonitor mMagazzino = new MagazzinoMonitor();
                mMagazzino.VerificaSaldiNegativi();
            }
            catch (Exception ex)
            {
                _log.Error("Errore Monitor Service", ex);
                while (ex.InnerException != null)
                {
                    _log.Error("--- INNER EXCEPTION", ex);
                    ex = ex.InnerException;
                }
            }
            finally
            {
            }
        }
    }
}
