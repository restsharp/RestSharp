using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Tests.SampleClasses
{
    public class EmployeeTracker
    {
        /// <summary>
        /// Key:    Employee name.
        /// Value:  Messages sent to employee.
        /// </summary>
        public Dictionary<String, List<String>> EmployeesMail { get; set; }
        /// <summary>
        /// Key:    Employee name.
        /// Value:  Hours worked this each week.
        /// </summary>
        public Dictionary<String, List<List<Int32>>> EmployeesTime { get; set; }

        /// <summary>
        /// Key:    Employee name.
        /// Value:  Payments made to employee
        /// </summary>
        public Dictionary<String, List<Payment>> EmployeesPay { get; set; }
    }

    public class Payment
    {
        public PaymentType Type { get; set; }
        public Int32 Amount { get; set; }
    }

    public enum PaymentType
    {
        Bonus,
        Monthly,
        BiWeekly
    }
}
