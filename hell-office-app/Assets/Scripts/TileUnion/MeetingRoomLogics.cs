﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Employee.Needs;
using Location;
using Location.EmployeeManager;
using Sirenix.OdinInspector;
using TileUnion.Tile;
using UnityEngine;

namespace TileUnion
{
    [AddComponentMenu("Scripts/TileUnion/TileUnion.MeetingRoomLogics")]
    public class MeetingRoomLogics : MonoBehaviour
    {
        private DataProvider<MeetingRoomPlaces> meetingRoomPlacesDataProvider;

        [SerializeField]
        private TileUnionImpl tileUnion;

        public TileUnionImpl TileUnion => tileUnion;

        [SerializeField]
        private List<TileImpl> tilesToAdd;

        public List<TileImpl> TilesToAdd => tilesToAdd;

        [SerializeField]
        private Direction growDirection;
        public Direction GrowDirection => growDirection;

        [SerializeField]
        private int maximumSize;

        [ReadOnly]
        [SerializeField]
        private int currentSize = 2;

        public int CurrentSize
        {
            get => currentSize;
            set => currentSize = value;
        }

        [ReadOnly]
        [SerializeField]
        private int employeePerGrow = 2;

        [SerializeField]
        private List<RoomTileLabel> incorrectMarks = new();
        public IEnumerable<RoomTileLabel> IncorrectMarks => incorrectMarks;

        private void Start()
        {
            meetingRoomPlacesDataProvider = new DataProvider<MeetingRoomPlaces>(
                () =>
                {
                    NeedProvider[] need_providers =
                        transform.GetComponentsInChildren<NeedProvider>();
                    return new MeetingRoomPlaces()
                    {
                        Places = need_providers
                            .Where(np => np.NeedType == NeedType.Meeting)
                            .ToList()
                    };
                },
                DataProviderServiceLocator.ResolveType.Singleton
            );
        }

        public bool IsEnoughPlace(int employeeCount)
        {
            return employeeCount <= (currentSize * employeePerGrow) - 1;
        }

        public bool IsCanFitEmployees(int employeeCount)
        {
            return employeeCount <= (maximumSize * employeePerGrow) - 1;
        }

        public int GetGrowCountForFitEmployees(int employeeCount)
        {
            if (IsEnoughPlace(employeeCount))
            {
                return 0;
            }
            else
            {
                int need = employeeCount - ((currentSize * employeePerGrow) - 1);
                return (need / employeePerGrow) + ((need % employeePerGrow) > 0 ? 1 : 0);
            }
        }

        public struct MeetingRoomGrowingInformation
        {
            public IEnumerable<Vector2Int> MovingTileUnionPositions;
            public IEnumerable<Vector2Int> PositionsToTake;
            public Vector2Int MovingDirection;
            public int MovingSteps;
        }

        public MeetingRoomGrowingInformation GetMeetingRoomGrowingInformation(int growCount)
        {
            Direction tempGrowDirection = GrowDirection;
            Enumerable
                .Range(0, TileUnion.Rotation)
                .ToList()
                .ForEach((x) => tempGrowDirection = tempGrowDirection.Rotate90());
            List<Vector2Int> positionsToTake = new();

            IEnumerable<Vector2Int> movingTileUnionPositions = (
                tempGrowDirection switch
                {
                    Direction.Up => TileUnion.TilesPositions.OrderByDescending(pos => pos.y),
                    Direction.Right => TileUnion.TilesPositions.OrderByDescending(pos => pos.x),
                    Direction.Down => TileUnion.TilesPositions.OrderBy(pos => pos.y),
                    Direction.Left => TileUnion.TilesPositions.OrderBy(pos => pos.x),
                    _ => throw new ArgumentException()
                }
            ).Take(TilesToAdd.Count());

            Vector2Int movingDirection = tempGrowDirection.ToVector2Int();
            positionsToTake.AddRange(movingTileUnionPositions.Select(x => x + movingDirection));

            for (int i = 0; i < growCount - 1; i++)
            {
                positionsToTake.AddRange(
                    positionsToTake
                        .TakeLast(movingTileUnionPositions.Count())
                        .Select(x => x + movingDirection)
                );
            }

            return new MeetingRoomGrowingInformation()
            {
                MovingTileUnionPositions = movingTileUnionPositions,
                PositionsToTake = positionsToTake,
                MovingDirection = movingDirection,
                MovingSteps = growCount
            };
        }

        public void AddTiles(MeetingRoomGrowingInformation info)
        {
            for (int i = 0; i < info.MovingSteps; i++)
            {
                TileUnion.MoveTiles(
                    info.MovingDirection,
                    info.MovingTileUnionPositions.Select(x => x + (info.MovingDirection * i))
                );
            }

            Dictionary<(Vector2Int position, int roatation), TileImpl> addingConfig = new();

            for (int i = 0; i < info.MovingSteps; i++)
            {
                for (int j = 0; j < info.MovingTileUnionPositions.Count(); j++)
                {
                    addingConfig.Add(
                        (
                            info.MovingTileUnionPositions.Select(x =>
                                x + (info.MovingDirection * i)
                            )
                                .ToList()[j],
                            TileUnion.Rotation
                        ),
                        TilesToAdd[j]
                    );
                }
                CurrentSize++;
            }
            TileUnion.AddTiles(addingConfig);

            Vector2Int unionPosition = TileUnion.Position;
            TileUnion.CreateCache(false);
            TileUnion.SetPosition(unionPosition);
        }
    }
}
