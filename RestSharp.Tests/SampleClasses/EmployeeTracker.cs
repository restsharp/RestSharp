using System.Collections.Generic;

namespace RestSharp.Tests.SampleClasses
{
    public class EmployeeTracker
    {
        /// <summary>
        /// Key:    Employee name.
        /// Value:  Messages sent to employee.
        /// </summary>
        public Dictionary<string, List<string>> EmployeesMail { get; set; }

        /// <summary>
        /// Key:    Employee name.
        /// Value:  Hours worked this each week.
        /// </summary>
        public Dictionary<string, List<List<int>>> EmployeesTime { get; set; }

        /// <summary>
        /// Key:    Employee name.
        /// Value:  Payments made to employee
        /// </summary>
        public Dictionary<string, List<Payment>> EmployeesPay { get; set; }
    }

    public class Payment
    {
        public PaymentType Type { get; set; }

        public int Amount { get; set; }
    }

    public enum PaymentType
    {
        Bonus,
        Monthly,
        BiWeekly
    }
}
