﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGameEngine
{
    public class CharacterData
    {
        [Header("Story Properties")]
        public string myName;
        public string myClassName;
        public CharacterRace race;
        public AudioProfileType audioProfile;

        [Header("Passive Properties")]
        public PassiveManagerModel passiveManager = new PassiveManagerModel();

        [Header("Health Properties")]
        public int health;
        public int maxHealth;

        [Header("Primary Attributes")]
        public int strength = 20;
        public int intelligence = 20;
        public int dexterity = 20;
        public int wits = 20;
        public int constitution = 20;

        [Header("Secondary Attributes")]
        public int stamina = 2;
        public int maxEnergy = 6;
        public int initiative = 0;
        public int draw = 4;
        public int power = 0;
        public int baseCrit = 0;
        public int critModifier = 30;
        public int baseFirstActivationDrawBonus = 0;

        [Header("XP + Level Properties")]
        public int currentLevel = 1;
        public int currentXP = 0;
        public int currentMaxXP;


        [Header("Deck Properties")]
        [HideInInspector] public List<CardData> deck;

        [Header("Model Properties")]
        public List<string> modelParts;

        [Header("Item Properties")]
        public ItemManagerModel itemManager = new ItemManagerModel();

        [Header("Talent Properties")]
        public int talentPoints = 0;
        public List<TalentPairingModel> talentPairings = new List<TalentPairingModel>();

        [Header("Attribute Points")]
        public int attributePoints = 0;
        public List<AttributeRollResult> attributeRollResults = new List<AttributeRollResult>();

        // Getters
        public int MaxHealthTotal
        {
            get { return (int)Math.Round(maxHealth * (constitution / 20f)); }
        }
    }
}