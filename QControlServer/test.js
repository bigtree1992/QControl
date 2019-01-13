var log4js = require('log4js');

var log4js_config = {
     "appenders":
        [
            {
                "category":"log_date",
                "type": "datefile",
                "filename": "./logs/log",
                "alwaysIncludePattern": true,
                "pattern": "-yyyy-MM-dd-hh.log"

            }
        ],
    "replaceConsole": true,
    "levels":
    {
        "log_date":"ALL"
    }
};

log4js.configure(log4js_config);

var logger = log4js.getLogger('log_date');

logger.info('hellefdas info');
logger.debug('dfasdfa debug');
logger.warn('sdasdf warn');
logger.error('dfads error');


