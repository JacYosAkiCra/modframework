using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Safe, read-only helper methods for accessing employee and team data from the game.
/// All methods include null-checks and error handling so they never crash your mod.
///
/// Usage:
///   var employees = ModEmployeeHelper.GetPlayerEmployees();
///   var teams = ModEmployeeHelper.GetPlayerTeams();
///   float salary = ModEmployeeHelper.GetTotalSalaryCost();
/// </summary>
namespace ModFramework.GameData
{
    public static class ModEmployeeHelper
    {
        /// <summary>
        /// Get all employees of the player's company.
        /// Returns an empty list if not in a game or if the player has no employees.
        /// </summary>
        public static List<Actor> GetPlayerEmployees()
        {
            var result = new List<Actor>();
            try
            {
                if (GameSettings.Instance == null || GameSettings.Instance.sActorManager == null)
                    return result;

                // Player employees belong to the player's teams
                var teams = GameSettings.Instance.sActorManager.Teams;
                if (teams != null)
                {
                    foreach (var team in teams.Values)
                    {
                        if (team != null)
                        {
                            var emps = team.GetEmployeesDirect();
                            if (emps != null)
                            {
                                foreach (var emp in emps)
                                {
                                    if (emp != null && !result.Contains(emp))
                                        result.Add(emp);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModEmployeeHelper.GetPlayerEmployees failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Get the total number of employees the player currently has.
        /// Returns 0 if not in a game.
        /// </summary>
        public static int GetPlayerEmployeeCount()
        {
            try
            {
                return GetPlayerEmployees().Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get all teams in the player's company.
        /// Returns an empty list if not in a game or if no teams exist.
        /// </summary>
        public static List<Team> GetPlayerTeams()
        {
            var result = new List<Team>();
            try
            {
                if (GameSettings.Instance == null || GameSettings.Instance.sActorManager == null)
                    return result;

                // Teams are stored on the ActorManager
                if (GameSettings.Instance.sActorManager.Teams != null)
                {
                    foreach (var team in GameSettings.Instance.sActorManager.Teams.Values)
                    {
                        if (team != null)
                            result.Add(team);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModEmployeeHelper.GetPlayerTeams failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Get the display name of an employee/actor.
        /// Returns "Unknown" if the actor is null.
        /// </summary>
        public static string GetName(Actor actor)
        {
            try
            {
                if (actor == null || actor.employee == null) return "Unknown";
                return actor.employee.Name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get the team name that an employee belongs to.
        /// Returns "Unassigned" if the employee has no team.
        /// </summary>
        public static string GetTeamName(Actor actor)
        {
            try
            {
                if (actor == null) return "Unassigned";

                if (GameSettings.Instance != null && GameSettings.Instance.sActorManager != null)
                {
                    var teams = GameSettings.Instance.sActorManager.Teams;
                    if (teams != null)
                    {
                        foreach (var team in teams.Values)
                        {
                            if (team != null)
                            {
                                var emps = team.GetEmployeesDirect();
                                if (emps != null && emps.Contains(actor))
                                {
                                    return team.Name ?? "Unassigned";
                                }
                            }
                        }
                    }
                }
                return "Unassigned";
            }
            catch
            {
                return "Unassigned";
            }
        }

        /// <summary>
        /// Check if the game is loaded and employee data is accessible.
        /// </summary>
        public static bool IsAvailable()
        {
            try
            {
                return GameSettings.Instance != null
                    && GameSettings.Instance.sActorManager != null
                    && GameSettings.Instance.MyCompany != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
