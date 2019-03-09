﻿using MonitorServices.Entities;
using MonitorServices.Properties;
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
        object lockObject = new object();
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
            if (Monitor.TryEnter(lockObject))
            {
                try
                {
                    MonitorService sMonitor = new MonitorService();
                    MonitorDS.MONITOR_SCHEDULERRow schedulazione;
                    if (sMonitor.VerificaEsecuzione("MAGAZZININEGATIVI", out schedulazione))
                    {
                        MagazzinoMonitor mMagazzino = new MagazzinoMonitor();
                        mMagazzino.VerificaSaldiNegativi();
                        sMonitor.AggiornaSchedulazione(schedulazione);
                    }

                    if (sMonitor.VerificaEsecuzione("MAGAZZINIGIACENZE", out schedulazione))
                    {
                        MagazzinoMonitor mMagazzino = new MagazzinoMonitor();
                        mMagazzino.VerificaGiacenze();
                        sMonitor.AggiornaSchedulazione(schedulazione);
                    }

                    if (sMonitor.VerificaEsecuzione("VERIFICAREPLICHE", out schedulazione))
                    {
                        VerificaRepliche mRepliche = new VerificaRepliche();
                        mRepliche.VerificaReplicheCartelleServer(@"\\DC01\IMMAGINI", @"\\DC02\IMMAGINI", "GIACOMO.BARLETTI", "topPasqua", "viamattei");
                        mRepliche.VerificaReplicheCartelleServer(@"\\DC01\DATI DISTRIBUITI", @"\\DC02\DATI DISTRIBUITI", "GIACOMO.BARLETTI", "topPasqua", "viamattei");
                        sMonitor.AggiornaSchedulazione(schedulazione);
                    }
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
                    Monitor.Exit(lockObject);
                }
            }
        }
    }
}
