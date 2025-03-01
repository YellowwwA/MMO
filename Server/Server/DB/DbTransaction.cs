﻿using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        //Me(GameRoom) -> You(Db) -> Me(GameRoom)
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me(GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            //You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext()) //db연동시마다 생성
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true; //db에서 hp변경
                    bool success = db.SaveChangesEx();

                    if(success)
                    {
                        //Me
                        room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
                    }
                }
            });

        }
        
        //Me(GameRoom)
        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me(GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
           
        }

        //You(Db)
        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext()) //db연동시마다 생성
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true; //db에서 hp변경
                bool success = db.SaveChangesEx();

                if (success)
                {
                    //Me
                    room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
                }
            }
        }

        //Me
        public static void SavePlayerStatus_Step3(int hp)
        {
            Console.WriteLine($"Hp Saved({hp})");
        }

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return;

            //살짝 문제있음
            //1)DB에 저장요청
            //2)DB저장 ok
            //3)메모리에 적용
            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };

            //You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext()) //db연동시마다 생성
                {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();

                    if (success)
                    {
                        //Me
                        room.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            player.Inven.Add(newItem);

                            //Client Noti
                            {
                                S_AddItem itemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.Info);
                                itemPacket.Items.Add(itemInfo);

                                player.Session.Send(itemPacket);
                            }
                        });
                    }
                }
            });
        }
    }
}
