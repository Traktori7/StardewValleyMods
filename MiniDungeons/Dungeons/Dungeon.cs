using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;


namespace MiniDungeons.Dungeons
{
	internal abstract class Dungeon : GameLocation
	{
		public bool cleared;
		public DungeonMap mapType;
		public Challenge challenge;

		/*public ReadOnlySpan<char> NameWithoutEnding
		{
			get
			{
				return Name.AsSpan(0, Name.IndexOf('_'));
			}
		}*/

		/*public string NameWithoutEnding
		{
			get
			{
				return Name[..Name.IndexOf('_')];
			}
		}*/

		public Dungeon(string name, DungeonMap mapType, Challenge challenge)
			: base(Path.Combine("Maps", name), name)
		{
			cleared = false;
			this.mapType = mapType;
			this.challenge = challenge;
		}


		public virtual void Initialize()
		{
			if (challenge is FightChallenge fightChallenge)
			{
				for (int i = 0; i < fightChallenge.MonsterWaves.Count; i++)
				{
					// TODO: UnHardoce this. Activator.CreateInstance or something
					MonsterWave wave = fightChallenge.MonsterWaves[i];

					for (int j = 0; j < wave.Monsters.Count; j++)
					{
						MonsterSpawn waveSpawn = wave.Monsters[j];

						for (int k = 0; k < waveSpawn.SpawnAmount; k++)
						{
							SpawnPoint point = PickSpawnPoint(fightChallenge.SpawnPoints);

							switch (waveSpawn.MonsterName)
							{
								case "GreenSlime":
									GreenSlime slime = new GreenSlime(new Vector2(point.X, point.Y) * 64f);
									characters.Add(slime);
									break;
								default:
									break;
							}
						}
					}
				}
			}
			else if (challenge is CollectChallenge collectChallenge)
			{
				// TODO: SpawnedObject.Count vs SpawnPoints.Count + randomly picking the points
				for (int i = 0; i < collectChallenge.SpawnedObjects.Count; i++)
				{
					SpawnedObject obj = collectChallenge.SpawnedObjects[i];

					SpawnPoint point = collectChallenge.SpawnPoints[i];

					objects.Add(new Vector2(point.X, point.Y), new StardewValley.Object(obj.ObjectID, 1));
				}
			}
		}


		public virtual void OnMonsterKilled(IEnumerable<NPC> monsters)
		{
			// This tries to test if all of the monsters have been killed, the player should still be in the location
			if (!characters.Any(c => c is Monster))
			{
				cleared = true;
			}
		}


		public static SpawnPoint PickSpawnPoint(List<SpawnPoint> points)
		{
			int rand = Game1.random.Next(points.Count);

			return points[rand];
		}
	}
}
