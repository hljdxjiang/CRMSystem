using System;
namespace CRMSystem.Helper
{
    public class LoggerTimes
    {
        private DateTime _dt;
        public LoggerTimes() {
            _dt = new DateTime();
        }

        public int ElapsedMilliseconds() {
            var now= new DateTime();
            var rs = now-_dt;
            return rs.Milliseconds;
        }
    }
}
