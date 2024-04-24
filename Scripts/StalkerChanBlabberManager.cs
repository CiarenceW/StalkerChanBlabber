using BepInEx;
using Receiver2;
using Receiver2ModdingKit.CustomSounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StalkerChanBlabber
{
	public class StalkerChanBlabberManager : MonoBehaviour
	{
		private const string kStalkerChanEvent = "event:/StalkerChan/";

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

		public void PlayBlabber(BlabberState blabberState)
		{
			StalkerChanVoiceline selectedVoiceline = voiceLineDictionnary[blabberState].Where( voiceline => !voiceline.playedLast ).ToList().GetRandom();

			foreach (StalkerChanVoiceline voiceline in voiceLineDictionnary[blabberState])
			{
				if (voiceline.playedLast) voiceline.playedLast = false;
			}

			selectedVoiceline.playedLast = true;

			ModAudioManager.PlayOneShotAttached(selectedVoiceline.eventName, LocalAimHandler.player_instance.gameObject);
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
						ModAudioManager.PlayOneShotAttached(selectedVoiceline.eventName, sneakBotStatue.gameObject);
					}
				}
			} 
		}
	}
}
