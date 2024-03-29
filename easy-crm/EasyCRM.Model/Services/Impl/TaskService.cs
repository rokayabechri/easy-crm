﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EasyCRM.Model.Domains;
using EasyCRM.Model.Repositories;
using EasyCRM.Model.Repositories.Entity;

namespace EasyCRM.Model.Services.Impl
{
    public class TaskService : ITaskService
    {
        private IValidationDictionary _validationDictionary;
        private ITaskRepository _repository;


        public TaskService(IValidationDictionary validationDictionary)
            : this(validationDictionary, new TaskEntityRepository())
        { }


        public TaskService(IValidationDictionary validationDictionary, ITaskRepository repository)
        {
            _validationDictionary = validationDictionary;
            _repository = repository;
        }


        public bool ValidateTask(Task taskToValidate)
        {
            //convert null values to default strings
            taskToValidate.Subject = taskToValidate.Subject ?? "";

            if (taskToValidate.Subject.Trim().Length == 0)
                _validationDictionary.AddError("Task.Subject", "A Subject is required.");

            if (taskToValidate.StartDate < DateTime.Now)
            {
                _validationDictionary.AddError("Task.StartDate", "The Start Date must be greater or equals to current date time.");
                return _validationDictionary.IsValid;
            }

            if (taskToValidate.EndDate <= taskToValidate.StartDate)
            {
                _validationDictionary.AddError("Task.EndDate", "The End Date must be greater than Start date.");
                return _validationDictionary.IsValid;
            }

            if (taskToValidate.LimitDate < taskToValidate.StartDate || taskToValidate.LimitDate > taskToValidate.EndDate)
                _validationDictionary.AddError("Task.LimitDate", "The Limit Date must be within Start Date and End Date interval.");



            return _validationDictionary.IsValid;
        }


        #region ITaskService Members

        public bool CreateTask(Task taskToCreate)
        {
            // Validation logic
            if (!ValidateTask(taskToCreate))
                return false;

            // Database logic
            try
            {
                _repository.Create(taskToCreate);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool EditTask(Task taskToEdit)
        {
            // we only validate the "Subject" as the other fields( StarDate,LimitDate,EndDate) cannot
            // be edited once the task has been created, and 'Status' and 'Priority' don't need to.
            if (taskToEdit.Subject.Trim().Length == 0)
            {
                _validationDictionary.AddError("Task.Subject", "A Subject is required.");
                return false;
            }

            // Database logic
            try
            {
                _repository.Update(taskToEdit);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool DeleteTask(Task taskToDelete)
        {
            try
            {
                _repository.Delete(taskToDelete);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public Task GetTask(int id)
        {
            try
            {
                return _repository.Get(id);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Task> ListTasks()
        {
            return _repository.ListAll();
        }

        public IEnumerable<Task> ListTasksByCriteria(Expression<Func<Task, bool>> predicate)
        {
            return _repository.ListAllByCriteria(predicate);
        }

        #endregion

    }
}
