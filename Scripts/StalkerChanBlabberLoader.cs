using BepInEx;
using HarmonyLib;
using Receiver2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Receiver2ModdingKit.Helpers;
using Receiver2ModdingKit;
using Receiver2.SneakBot;
using UnityEngine.Events;
using Wolfire;
using System.IO;
using Receiver2ModdingKit.CustomSounds;
using ImGuiNET;
using BepInEx.Configuration;

namespace StalkerChanBlabber
{
	[BepInPlugin("TCCrew.StalkerChanBlabber", "StalkerChanBlabber", "1.0.0")]
	public class StalkerChanBlabberLoader : BaseUnityPlugin
	{
		internal static ConfigEntry<float> stalkerChanVolumeMultiplier;

		public void Awake()
		{
			stalkerChanVolumeMultiplier = Config.Bind("StalkerChanBlabber", "VolumePercentage", 0.4f, new ConfigDescription("Volume multiplier for Stalker-Chan. Note that this is the same option as the one in the settings menu", new AcceptableValueRange<float>(0, 1)));

			StalkerChanBlabberManager.volumeMultiplier = stalkerChanVolumeMultiplier.Value;

			stalkerChanVolumeMultiplier.SettingChanged += ((s, e) => StalkerChanBlabberManager.volumeMultiplier = ((ConfigEntry<float>)s).Value);

			var attribute = (BepInPlugin)Attribute.GetCustomAttribute(this.GetType(), typeof(BepInPlugin));

			Logger.LogInfo($"Plugin { attribute.Name } version { attribute.Version } is loaded!");

			if (!gameObject.GetComponent<StalkerChanBlabberManager>())
			{
				gameObject.AddComponent<StalkerChanBlabberManager>();
			}

			var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			using (BinaryReader stalkerChanBlabberBankReader = new BinaryReader(new FileStream(directoryPath + "/StalkerChanBlabberBank.bank", FileMode.Open)))
			{
				byte[] stalkerChanBlabberBankBuffer = new byte[stalkerChanBlabberBankReader.BaseStream.Length];

				stalkerChanBlabberBankReader.Read(stalkerChanBlabberBankBuffer, 0, stalkerChanBlabberBankBuffer.Length);

				ModAudioManager.mod_system.loadBankMemory(stalkerChanBlabberBankBuffer, FMOD.Studio.LOAD_BANK_FLAGS.UNENCRYPTED, out var stalkerChanBlabberBank);

				Receiver2ModdingKit.CustomSounds.Utility.IsError(stalkerChanBlabberBank.loadSampleData(), "BlabberBank is fucked");
			}

			using (BinaryReader stalkerChanBlabberStringsReader = new BinaryReader(new FileStream(directoryPath + "/StalkerChanBlabberBank.strings.bank", FileMode.Open)))
			{
				byte[] stalkerChanBlabberStringsBuffer = new byte[stalkerChanBlabberStringsReader.BaseStream.Length];

				stalkerChanBlabberStringsReader.Read(stalkerChanBlabberStringsBuffer, 0, stalkerChanBlabberStringsBuffer.Length);

				ModAudioManager.mod_system.loadBankMemory(stalkerChanBlabberStringsBuffer, FMOD.Studio.LOAD_BANK_FLAGS.UNENCRYPTED, out var stalkerChanBlabberStrings);

				Receiver2ModdingKit.CustomSounds.Utility.IsError(stalkerChanBlabberStrings.loadSampleData(), "BlabberStrings is fucked");
			}

			Receiver2ModdingKit.ModdingKitEvents.AddTaskAtCoreStartup(CreateSettings);

			ReceiverEvents.StartListening(ReceiverEventTypeVoid.PlayerInitialized, PlayerInitialized);

			Harmony.CreateAndPatchAll(this.GetType());
		}

		private void CreateSettings()
		{
			var stalkerChanVolumeSettingEntry = Receiver2ModdingKit.SettingsMenuManager.CreateSettingsMenuOption<float>("Stalker-Chan Volume Multiplier", stalkerChanVolumeMultiplier, 8);

			var volumeSliderComponent = stalkerChanVolumeSettingEntry.control.GetComponent<SliderComponent>();

			volumeSliderComponent.OnChange.AddListener((value) => stalkerChanVolumeMultiplier.Value = value);

			volumeSliderComponent.format = "P0";

			volumeSliderComponent.Value = stalkerChanVolumeMultiplier.Value;
		}

		private void PlayerInitialized(ReceiverEventTypeVoid ev)
		{
			var weapStoreRoom = GameObject.Find("Weapon Storage Room");
			if (weapStoreRoom)
			{
				var statues = weapStoreRoom.transform.Find("Statues");
				if (statues) 
				{
					var sneakBotStatue = statues.Find("SneakBotStatue");
					if (sneakBotStatue)
					{
						var easterEgg = new GameObject("Easter Egg");

						easterEgg.transform.parent = sneakBotStatue.transform;

						easterEgg.transform.localPosition = Vector3.zero.SetY(0.1f);

						easterEgg.layer = Layers.Interactable;

						easterEgg.AddComponent<SphereCollider>().radius = 1f;

						var button = easterEgg.AddComponent<PressableButton>();

						var pressableButtonEvent = new PressableButtonUnityEvent();
						pressableButtonEvent.AddListener(StalkerChanBlabberManager.Instance.PlayEasterEggBlabber);

						button.onPress = pressableButtonEvent;
					}
				}
			}
		}

		[HarmonyPatch(typeof(MenuManagerScript), "UpdateDeveloperMenu")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> AddAudioDebug(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
		{
			CodeMatcher codeMatcher = new CodeMatcher(instructions, generator)
				.MatchForward(false,
				new CodeMatch(OpCodes.Ldstr, "Debug Text Window"),
				new CodeMatch(OpCodes.Ldstr, ""),
				new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ConfigFiles), nameof(ConfigFiles.global))),
				new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ConfigFiles.Global), nameof(ConfigFiles.Global.display_debug_text_window))),
				new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ImGui), nameof(ImGui.MenuItem), new Type[] { typeof(string), typeof(string), typeof(bool) })),
				new CodeMatch(OpCodes.Brfalse)
				);

			if (!codeMatcher.ReportFailure(__originalMethod, Debug.LogError))
			{
				var labels = codeMatcher.Instruction.ExtractLabels();

				codeMatcher.Insert(new CodeInstruction[]
				{
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StalkerChanBlabberManager), nameof(StalkerChanBlabberManager.OpenAudioDebug)))
				});

				codeMatcher.Labels.AddRange(labels);
			}

			return codeMatcher.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(SneakBotTape), nameof(SneakBotTape.StartEvent))]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> SayTapeBlabber(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
		{
			CodeMatcher codeMatcher = new CodeMatcher(instructions, generator)
				.MatchForward(false,
				new CodeMatch(OpCodes.Ldloc_0),
				new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(RuntimeTileLevelGenerator), nameof(RuntimeTileLevelGenerator.CreateSneakBot)))
				);

			if (!codeMatcher.ReportFailure(__originalMethod, Debug.LogError))
			{
				codeMatcher.InsertAndAdvance(
					AccessTools.Method(typeof(StalkerChanBlabberLoader), nameof(StalkerChanBlabberLoader.PlaySummonedByTape)).ToCodeInstructionsClipLast(out _)
					);
			}

			return codeMatcher.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(RankingProgressionGameMode), "OnEntityDestroyed")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> SayPumpkingBlabber(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
		{
			CodeMatcher codeMatcher = new CodeMatcher(instructions, generator)
				.MatchForward(false, 
				new CodeMatch(OpCodes.Ldloc_0),
				new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(RuntimeTileLevelGenerator), nameof(RuntimeTileLevelGenerator.CreateSneakBot)))
				);

			if (!codeMatcher.ReportFailure(__originalMethod, Debug.LogError))
			{
				codeMatcher.InsertAndAdvance(
					AccessTools.Method(typeof(StalkerChanBlabberLoader), nameof(StalkerChanBlabberLoader.PlaySummonedByPumpkin)).ToCodeInstructionsClipLast(out _)
					);
			}

			return codeMatcher.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(SneakBotSpawner), nameof(SneakBotSpawner.TrySpawnSneakBotInstanced))]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> SayGongBlabber(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
		{
			CodeMatcher codeMatcher = new CodeMatcher(instructions, generator).Start();

			if (!codeMatcher.ReportFailure(__originalMethod, Debug.LogError))
			{
				codeMatcher
					.InsertAndAdvance(
					AccessTools.Method(typeof(StalkerChanBlabberLoader), nameof(StalkerChanBlabberLoader.PlaySummonedByGong)).ToCodeInstructionsClipLast(out var labels)
					)
					.AddLabels(labels)
					;
			}

			return codeMatcher.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(SneakBot), nameof(SneakBot.OnHackProgress))]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> SayHackBlabber(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
		{
			CodeMatcher codeMatcher = new CodeMatcher(instructions, generator)
				.MatchStartForward(new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldc_I4_1), new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(SneakBot), "being_hacked")));

			if (!codeMatcher.ReportFailure(__originalMethod, Debug.LogError))
			{
				codeMatcher
					.InsertAndAdvance(AccessTools.Method(typeof(StalkerChanBlabberLoader), nameof(StalkerChanBlabberLoader.PlayEnterHack)).ToCodeInstructionsClipLast(out _));
			}

			return codeMatcher.InstructionEnumeration();
		}

		private void PlaySummonedByPumpkin()
		{
			Debug.Log("plumpkin!!!");

			StalkerChanBlabberManager.Instance.PlayBlabber(BlabberState.SummonedByPumpkin);
		}

		private void PlaySummonedByTape()
		{
			Debug.Log("Taep");

			StalkerChanBlabberManager.Instance.PlayBlabber(BlabberState.SummonedByTape);
		}

		private void PlaySummonedByGong()
		{
			Debug.Log("gnong?");

			if (this.name == "Gong")
			{
				Debug.Log("gnong!");
				StalkerChanBlabberManager.Instance.PlayBlabber(BlabberState.SummonedByGong);
			}
		}

		private void PlayEnterHack()
		{
			Debug.Log("hack");

			StalkerChanBlabberManager.Instance.PlayBlabber(BlabberState.Hacking);
		}

		[HarmonyPatch(typeof(SneakBotTeleportationScript), nameof(SneakBotTeleportationScript.TeleportStalk))]
		[HarmonyPostfix]
		private static void PlayStalkingBlabber(SneakBotTeleportationScript __instance)
		{
			Debug.Log("stalk?");

			if (Receiver2.Probability.Chance(.5f))
			{
				Debug.Log("stalk!");

				StalkerChanBlabberManager.Instance.PlayBlabber(BlabberState.Stalking);
			}
		}

		[HarmonyPatch(typeof(SneakBot), "PlayDeathSound")]
		[HarmonyPostfix]
		private static void PlayDeathBlabber()
		{
			Debug.Log("death");

			StalkerChanBlabberManager.Instance.PlayBlabber(BlabberState.Death);
		}
	}

	public static class Extensions
	{
		public static List<CodeInstruction> ToCodeInstructionsClipLast(this System.Reflection.MethodInfo methodInfo, out List<Label> extractedLabels, Dictionary<CodeInstruction, CodeInstruction> replaceInstructionWith = null)
		{
			var methodIL = PatchProcessor.GetOriginalInstructions(methodInfo);

			if (replaceInstructionWith != null)
			{
				for (int instructionIndex = 0; instructionIndex < methodIL.Count; instructionIndex++)
				{
					if (replaceInstructionWith.TryGetValue(methodIL[instructionIndex], out var replacementInstrution))
					{
						methodIL[instructionIndex] = replacementInstrution;
					}
				}
			}

			extractedLabels = (methodIL.Last().ExtractLabels());
			methodIL.Remove(methodIL.Last());

			return methodIL;
		}
	}
}
