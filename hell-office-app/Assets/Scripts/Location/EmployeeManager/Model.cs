﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Employee;
using Employee.Needs;
using Employee.Personality;
using Level;
using Level.Boss.Task;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using TileBuilderController = TileBuilder.Controller;

namespace Location.EmployeeManager
{
    public struct MeetingRoomPlaces
    {
        public List<NeedProvider> Places;
    }

    [AddComponentMenu("Scripts/Location/EmployeeManager/Location.EmployeeManager.Model")]
    public class Model : MonoBehaviour
    {
        private DataProvider<EmployeeAmount> employeeAmountDataProvider;
        private DataProvider<MaxStress> maxStressDataProvider;
        private DataProvider<AllEmployeesAtMeeting> allEmployeesAtMeetingDataProvider;
        private DataProvider<AllEmployeesAtBadroom> allEmployeesAtBadroomDataProvider;

        [SerializeField]
        private UnityEvent<EmployeeImpl> employeeFired;

        [SerializeField]
        private EmployeeImpl employeePrototype;

        [SerializeField]
        [Required]
        private NeedProviderManager needProviderManager;

        [SerializeField]
        [Required]
        private Level.Finances.Model finances;

        [SerializeField]
        private TileBuilderController tileBuilderController;

        [ReadOnly]
        [SerializeField]
        private List<EmployeeImpl> employees = new();

        public delegate void EmployeeNeedSatisfiedHandler(EmployeeImpl employee, NeedType need);
        public event EmployeeNeedSatisfiedHandler EmployeeNeedSatisfied;

        private void Start()
        {
            employeeAmountDataProvider = new DataProvider<EmployeeAmount>(
                () => new EmployeeAmount { Amount = employees.Count },
                DataProviderServiceLocator.ResolveType.Singleton
            );
            maxStressDataProvider = new DataProvider<MaxStress>(
                () =>
                {
                    float max_stress = float.NegativeInfinity;
                    foreach (EmployeeImpl emp in employees)
                    {
                        if (emp.Stress.Stress > max_stress)
                        {
                            max_stress = emp.Stress.Stress;
                        }
                    }

                    return new MaxStress { Stress = max_stress };
                },
                DataProviderServiceLocator.ResolveType.Singleton
            );
            allEmployeesAtMeetingDataProvider = new DataProvider<AllEmployeesAtMeeting>(
                () =>
                {
                    bool all_at_meeting = employees.All(employee =>
                        employee.CurrentNeedType == NeedType.Meeting
                    );
                    return new AllEmployeesAtMeeting { Value = all_at_meeting };
                },
                DataProviderServiceLocator.ResolveType.Singleton
            );
            allEmployeesAtBadroomDataProvider = new DataProvider<AllEmployeesAtBadroom>(
                () =>
                {
                    bool all_go_home = employees.All(employee =>
                        employee.CurrentNeedType == NeedType.Sleep
                    );
                    return new AllEmployeesAtBadroom { Value = all_go_home };
                },
                DataProviderServiceLocator.ResolveType.Singleton
            );
        }

        // TODO: Remove it when employee serialization will be implemented (#121)
        public IEnumerator TurnOnAllEmployees(float delay)
        {
            foreach (EmployeeImpl employee in employees)
            {
                employee.gameObject.SetActive(true);
                yield return delay;
            }
        }

        public Result AddEmployee(PersonalityImpl personality)
        {
            Result result = tileBuilderController.GrowMeetingRoomForEmployees(employees.Count + 1);

            if (result.Failure)
            {
                return result;
            }

            EmployeeImpl employee = Instantiate(employeePrototype, transform)
                .GetComponent<EmployeeImpl>();

            employee.gameObject.SetActive(true);
            employee.SetPersonality(personality);
            employee.NeedProviderManager = needProviderManager;
            employee.IncomeGenerator.Finances = finances;

            MeetingRoomPlaces meeting_room_places =
                DataProviderServiceLocator.FetchDataFromSingleton<MeetingRoomPlaces>();

            foreach (NeedProvider place in meeting_room_places.Places)
            {
                bool taken = employee.TryForceTakeNeedProvider(place);
                if (!taken)
                {
                    continue;
                }

                employees.Add(employee);
                employee.NeedSatisifed += (need) => EmployeeNeedSatisfied?.Invoke(employee, need);
                return new SuccessResult();
            }

            Destroy(employee.gameObject);
            return new FailResult("Cannot find place in meeting room");
        }

        public void FireEmployee(EmployeeImpl employee)
        {
            if (employee.CurrentNeedType != NeedType.Meeting)
            {
                Debug.LogError("Cannot fire employee that's not on meeting");
                return;
            }

            employeeFired.Invoke(employee);
            _ = employees.Remove(employee);
            Destroy(employee.gameObject);
        }
    }
}
