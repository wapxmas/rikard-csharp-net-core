using RikardLib.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspCron
{
    public class CronAction
    {
        private readonly Action action;
        private readonly Logger logger = new Logger();
        private readonly string actionName;

        public CronAction(Action action, string actionName)
        {
            this.action = action;
            this.actionName = actionName;
        }

        public void Action()
        {
            logger.Info($"Cron: {actionName}");

            try
            {
                action();
            }
            catch(Exception e)
            {
                logger.Error($"While executing action {actionName}.", e);
            }
        }
    }
}
