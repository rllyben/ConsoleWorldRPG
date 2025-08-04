using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Services
{
    public static class DayCycleManager
    {
        private static DateOnly _lastDay; 
        private static bool _running = false;
        public static TimeSegment CurrentTimeSegment { get; private set; }
        /// <summary>
        /// initialises the session time
        /// </summary>
        public static void Initialize()
        {
            var now = DateTime.Now;
            _lastDay = DateOnly.FromDateTime(now);
            UpdateTimeSegment(now.Hour);

            Console.WriteLine($"🌅 New session started. Time: {now:HH:mm} ({CurrentTimeSegment})");

            // Roll gathering limits for the new day
            foreach (var room in RoomService.AllRooms)
            {
                if (room.GatheringSpots.Any())
                    room.RollGatherLimit();
            }

        }
        /// <summary>
        /// starts the background time sync
        /// </summary>
        /// <param name="intervalMs"></param>
        public static void StartBackgroundLoop(int intervalMs = 30000)
        {
            if (_running) return;
            _running = true;

            Task.Run(async () =>
            {
                while (_running)
                {
                    try
                    {
                        Update();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("🛑 DayCycleManager error: " + ex.Message);
                    }

                    await Task.Delay(intervalMs);
                }

            });

        }
        /// <summary>
        /// Updates the Session time
        /// </summary>
        public static void Update()
        {
            var now = DateTime.Now;

            if (DateOnly.FromDateTime(now) != _lastDay)
            {
                _lastDay = DateOnly.FromDateTime(now);
                Console.WriteLine("📆 A new day has begun.");

                foreach (var room in RoomService.AllRooms)
                {
                    if (room.GatheringSpots.Any())
                        room.RollGatherLimit();
                }

            }

            UpdateTimeSegment(now.Hour);
        }
        /// <summary>
        /// Updates the day time segement
        /// </summary>
        /// <param name="hour">current hour</param>
        private static void UpdateTimeSegment(int hour)
        {
            CurrentTimeSegment = hour switch
            {
                >= 6 and < 12 => TimeSegment.Morning,
                >= 12 and < 17 => TimeSegment.Midday,
                >= 17 and < 21 => TimeSegment.Evening,
                _ => TimeSegment.Night
            };

        }

    }

}
