using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversationEditor
{
    public class DataSource
    {
        private readonly Character[] m_characters = new Character[]
            {
                Character.Default,
                new Character("Shane"),
                new Character("Neil"),
                new Character("Tammy"),
            };

        public Character GetCharacter(string name)
        {
            return m_characters.First(c => c.Name == name);
        }

        public IEnumerable<Character> Characters
        {
            get { return m_characters; }
        }

        private readonly Func<Condition>[] m_conditions = new Func<Condition>[] 
            {
                ()=> new Condition("IsAlive", new CharacterParameter("NPC", Character.Default) ),
                ()=> new Condition("IsDead",  new CharacterParameter("NPC", Character.Default) ),
                //new Condition() { Name = "PlayerHasItem", ParameterNames = new string[] { "Item" } },
                //new Condition() { Name = "PlayerDoesntHaveItem", ParameterNames = new string[] { "Item" } },
                ()=> new Condition("KilledBy", new CharacterParameter("Killed", Character.Default), new CharacterParameter("Killer", Character.Default)),
                ()=> new Condition("TimeInSpielPassed", new TimeParameter("Time", new Time(0))),
                ()=> new Condition("TimeInSpielNotPassed", new TimeParameter("Time", new Time(0))),
                ()=> new Condition("Else"),
            };

        public Condition GetCondition(string name)
        {
            return m_conditions.First(c => c().Name == name)();
        }

        public IEnumerable<Func<Condition>> Conditions
        {
            get { return m_conditions; }
        }
    }
}
