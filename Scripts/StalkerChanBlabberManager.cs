using BepInEx;
using FMOD.Studio;
using FMOD;
using FMODUnity;
using ImGuiNET;
using Receiver2;
using Receiver2ModdingKit.CustomSounds;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StalkerChanBlabber
{
	public class StalkerChanBlabberManager : MonoBehaviour
	{
		private const string kStalkerChanEvent = "event:/StalkerChan/";

		private static bool debugWindowVisible;

		internal static float volumeMultiplier = 0.4f;

		public Dictionary<BlabberState, List<StalkerChanVoiceline>> voiceLineDictionnary = new Dictionary<BlabberState, List<StalkerChanVoiceline>>
		{
			{ BlabberState.SummonedByGong, new List<StalkerChanVoiceline>
				{
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "Prey" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "Cocky" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "Neighbours"}
				}
			},

			{ BlabberState.SummonedByTape, new List<StalkerChanVoiceline> 
				{ 
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "FoundYa" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "TheMan" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "Ready" }
				}
			},

			{ BlabberState.SummonedByPumpkin, new List<StalkerChanVoiceline>
				{
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "Griddy" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "Trick" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "EasyTarget"}
				} 
			},

			{ BlabberState.Stalking, new List<StalkerChanVoiceline>
				{
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "BehindYou" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "GunJam" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "PutDownGunAndGiveUp" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "GunOilAndFear" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "CuteWhenAfraid" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "GettingCloser" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "AraAraUrgh" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "CatAndMouse" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "JohnMosesBrowning" }
				}
			},

			{ BlabberState.Hacking, new List<StalkerChanVoiceline> 
				{
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "Bastard" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "DontTouch" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "LetGo" }
				}
			},

			{ BlabberState.Death, new List<StalkerChanVoiceline>
				{
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "NondescriptDeath1" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "NondescriptDeath2" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "NondescriptDeath3" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "SpicyDeath1" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "SpicyDeath2" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "SpicyDeath3" }
				} 
			},

			{ BlabberState.EasterEgg, new List<StalkerChanVoiceline> 
				{
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "TheSigns" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "GetPB" },
					new StalkerChanVoiceline() { eventName = kStalkerChanEvent + "ReadCredits" }
				}
			}
		};

		public static StalkerChanBlabberManager Instance 
		{ 
			get; 
			private set; 
		}

		public void Awake()
		{
			Instance = this;
		}

		public void Update()
		{
			if (debugWindowVisible)
			{
				ImGui.SetNextWindowSize(new Vector2(440f, 550f), ImGuiCond.FirstUseEver);
				if (ImGui.Begin("StalkerChanBlabber", ref debugWindowVisible))
				{
					ImGui.SliderFloat("Audio Boost", ref volumeMultiplier, 0.01f, 100f);
				}
				ImGui.End();
			}
		}

		public static void OpenAudioDebug()
		{
			if (ImGui.MenuItem("StalkerChanBlabber Audio Debug", "", debugWindowVisible))
			{
				debugWindowVisible = !debugWindowVisible;
			}
		}

		public void PlayBlabber(BlabberState blabberState)
		{
			StalkerChanVoiceline selectedVoiceline = voiceLineDictionnary[blabberState].Where( voiceline => !voiceline.playedLast ).ToList().GetRandom();

			foreach (StalkerChanVoiceline voiceline in voiceLineDictionnary[blabberState])
			{
				if (voiceline.playedLast) voiceline.playedLast = false;
			}

			selectedVoiceline.playedLast = true;

			ModAudioManager.PlayOneShotAttached(selectedVoiceline.eventName, LocalAimHandler.player_instance.gameObject, 1 * volumeMultiplier);
		}

		public void PlayEasterEggBlabber()
		{
			StalkerChanVoiceline selectedVoiceline = voiceLineDictionnary[BlabberState.EasterEgg].Where(voiceline => !voiceline.playedLast).ToList().GetRandom();

			foreach (StalkerChanVoiceline voiceline in voiceLineDictionnary[BlabberState.EasterEgg])
			{
				if (voiceline.playedLast) voiceline.playedLast = false;
			}

			selectedVoiceline.playedLast = true;

			var weapStoreRoom = GameObject.Find("Weapon Storage Room");
			if (weapStoreRoom)
			{
				var statues = weapStoreRoom.transform.Find("Statues");
				if (statues)
				{
					var sneakBotStatue = statues.Find("SneakBotStatue");
					if (sneakBotStatue)
					{
						ModAudioManager.PlayOneShotAttached(selectedVoiceline.eventName, sneakBotStatue.gameObject, 1 * volumeMultiplier);
					}
				}
			} 
		}
	}
}
