
using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SimpleStation
{
    using static Program;

    public class StationNodeManager : CustomNodeManager2
    {
        public enum StationStatus : int
        {
            Ready = 0,
            WorkInProgress = 1,
            Done = 2,
            Discarded = 3,
            Fault = 4
        }

        public static double PowerConsumption { get; set; } = POWERCONSUMPTION_DEFAULT;

        public static ulong IdealCycleTimeDefault { get; set; } = IDEAL_CYCLETIME_DEFAULT;

        public static bool GenerateAlerts { get; set; } = false;

        public int CurrentShift
        {
            get => (int)_currentShift.Value;
            set
            {
                _currentShift.Value = value;
                _currentShift.Timestamp = DateTime.Now;
                _currentShift.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong ProductSerialNumber
        {
            get => (ulong)_productSerialNumber.Value;
            set
            {
                _productSerialNumber.Value = value;
                _productSerialNumber.Timestamp = DateTime.Now;
                _productSerialNumber.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong NumberOfManufacturedProducts
        {
            get => (ulong)_numberOfManufacturedProducts.Value;
            set
            {
                _numberOfManufacturedProducts.Value = value;
                _numberOfManufacturedProducts.Timestamp = DateTime.Now;
                _numberOfManufacturedProducts.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong NumberOfManufacturedProductsInShift
        {
            get => (ulong)_numberOfManufacturedProductsInShift.Value;
            set
            {
                _numberOfManufacturedProductsInShift.Value = value;
                _numberOfManufacturedProductsInShift.Timestamp = DateTime.Now;
                _numberOfManufacturedProductsInShift.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong NumberOfDiscardedProducts
        {
            get => (ulong)_numberOfDiscardedProducts.Value;
            set
            {
                _numberOfDiscardedProducts.Value = value;
                _numberOfDiscardedProducts.Timestamp = DateTime.Now;
                _numberOfDiscardedProducts.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong NumberOfDiscardedProductsInShift
        {
            get => (ulong)_numberOfDiscardedProductsInShift.Value;
            set
            {
                _numberOfDiscardedProductsInShift.Value = value;
                _numberOfDiscardedProductsInShift.Timestamp = DateTime.Now;
                _numberOfDiscardedProductsInShift.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong OverallRunningTime
        {
            get => (ulong)_overallRunningTime.Value;
            set
            {
                _overallRunningTime.Value = value;
                _overallRunningTime.Timestamp = DateTime.Now;
                _overallRunningTime.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong FaultyTime
        {
            get => (ulong)_faultyTime.Value;
            set
            {
                _faultyTime.Value = value;
                _faultyTime.Timestamp = DateTime.Now;
                _faultyTime.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong NumberOfFaults
        {
            get => (ulong)_numberOfFaults.Value;
            set
            {
                _numberOfFaults.Value = value;
                _numberOfFaults.Timestamp = DateTime.Now;
                _numberOfFaults.ClearChangeMasks(SystemContext, false);
            }
        }
        public ulong NumberOfFaultsInShift
        {
            get => (ulong)_numberOfFaultsInShift.Value;
            set
            {
                _numberOfFaultsInShift.Value = value;
                _numberOfFaultsInShift.Timestamp = DateTime.Now;
                _numberOfFaultsInShift.ClearChangeMasks(SystemContext, false);
            }
        }

        public StationStatus StationState
        {
            get => (StationStatus)_stationState.Value;
            set
            {
                _stationState.Value = value;
                _stationState.Timestamp = DateTime.Now;
                _stationState.ClearChangeMasks(SystemContext, false);
            }
        }

        public double EnergyConsumption
        {
            get => (double)_energyConsumption.Value;
            set
            {
                _energyConsumption.Value = value;
                _energyConsumption.Timestamp = DateTime.Now;
                _energyConsumption.ClearChangeMasks(SystemContext, false);
            }
        }

        public double Pressure
        {
            get => (double)_pressure.Value;
            set
            {
                _pressure.Value = value;
                _pressure.Timestamp = DateTime.Now;
                _pressure.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong IdealCycleTime
        {
            get => (ulong)_idealCycleTime.Value;
            set
            {
                _idealCycleTime.Value = value;
                _idealCycleTime.Timestamp = DateTime.Now;
                _idealCycleTime.ClearChangeMasks(SystemContext, false);
            }
        }

        public ulong ActualCycleTime
        {
            get => (ulong)_actualCycleTime.Value;
            set
            {
                _actualCycleTime.Value = value;
                _actualCycleTime.Timestamp = DateTime.Now;
                _actualCycleTime.ClearChangeMasks(SystemContext, false);
            }
        }

        public StationNodeManager(Opc.Ua.Server.IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration, Namespaces.StationApplications)
        {
            SystemContext.NodeIdFactory = this;
        }

        /// <summary>
        /// Creates the NodeId for the specified node.
        /// </summary>
        public override NodeId New(ISystemContext context, NodeState node)
        {
            if (node is BaseInstanceState instance && instance.Parent != null)
            {
                if (instance.Parent.NodeId.Identifier is string id)
                {
                    return new NodeId(id + "_" + instance.SymbolicName, instance.Parent.NodeId.NamespaceIndex);
                }
            }

            return node.NodeId;
        }

        /// <summary>
        /// Creates a new folder.
        /// </summary>
        private FolderState CreateFolder(NodeState parent, string path, string name)
        {
            FolderState folder = new FolderState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId(path, NamespaceIndex),
                BrowseName = new QualifiedName(path, NamespaceIndex),
                DisplayName = new LocalizedText("en", name),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None
            };

            parent?.AddChild(folder);

            return folder;
        }

        /// <summary>
        /// Does any initialization required before the address space can be used.
        /// </summary>
        /// <remarks>
        /// The externalReferences is an out parameter that allows the node manager to link to nodes
        /// in other node managers. For example, the 'Objects' node is managed by the CoreNodeManager and
        /// should have a reference to the root folder node(s) exposed by this node manager.
        /// </remarks>
        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
                {
                    externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                }

                FolderState root = CreateFolder(null, "SimpleStation", "SimpleStation");
                root.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);
                references.Add(new NodeStateReference(ReferenceTypes.Organizes, false, root.NodeId));
                root.EventNotifier = EventNotifiers.SubscribeToEvents;
                AddRootNotifier(root);

                List<BaseDataVariableState> variables = new List<BaseDataVariableState>();

                try
                {
                    FolderState dataFolder = CreateFolder(root, "Telemetry", "Telemetry");

                    _currentShift = CreateBaseVariable(dataFolder, "CurrentShift", "CurrentShift", BuiltInType.Int32, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _productSerialNumber = CreateBaseVariable(dataFolder, "ProductSerialNumber", "ProductSerialNumber", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _numberOfManufacturedProducts = CreateBaseVariable(dataFolder, "NumberOfManufacturedProducts", "NumberOfManufacturedProducts", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _numberOfDiscardedProducts = CreateBaseVariable(dataFolder, "NumberOfDiscardedProducts", "NumberOfDiscardedProducts", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _numberOfManufacturedProductsInShift = CreateBaseVariable(dataFolder, "NumberOfManufacturedProductsInShift", "NumberOfManufacturedProductsInShift", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _numberOfDiscardedProductsInShift = CreateBaseVariable(dataFolder, "NumberOfDiscardedProductsInShift", "NumberOfDiscardedProductsInShift", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _overallRunningTime = CreateBaseVariable(dataFolder, "OverallRunningTime", "OverallRunningTime", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _faultyTime = CreateBaseVariable(dataFolder, "FaultyTime", "FaultyTime", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _numberOfFaults = CreateBaseVariable(dataFolder, "NumberOfFaults", "NumberOfFaults", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _numberOfFaultsInShift = CreateBaseVariable(dataFolder, "NumberOfFaultsInShift", "NumberOfFaultsInShift", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _stationState = CreateBaseVariable(dataFolder, "Status", "Status", BuiltInType.Integer, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    StationState = StationStatus.Ready;
                    _energyConsumption = CreateBaseVariable(dataFolder, "EnergyConsumption", "EnergyConsumption", BuiltInType.Double, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    _pressure = CreateBaseVariable(dataFolder, "Pressure", "Pressure", BuiltInType.Double, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    Pressure = PRESSURE_DEFAULT;
                    _idealCycleTime = CreateBaseVariable(dataFolder, "IdealCycleTime", "IdealCycleTime", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    IdealCycleTime = IdealCycleTimeDefault;
                    _actualCycleTime = CreateBaseVariable(dataFolder, "ActualCycleTime", "ActualCycleTime", BuiltInType.UInt64, ValueRanks.Scalar, AccessLevels.CurrentRead);
                    ActualCycleTime = IdealCycleTimeDefault;

                    FolderState methodsFolder = CreateFolder(root, "Methods", "Methods");

                    MethodState executeMethod = CreateMethod(methodsFolder, "Execute", "Execute");
                    SetExecuteMethodProperties(ref executeMethod);

                    MethodState resetMethod = CreateMethod(methodsFolder, "Reset", "Reset");
                    SetResetMethodProperties(ref resetMethod);

                    MethodState openPressureReleaseValveMethod = CreateMethod(methodsFolder, "OpenPressureReleaseValve", "OpenPressureReleaseValve");
                    SetOpenPressureReleaseValveMethodProperties(ref openPressureReleaseValveMethod);
                }
                catch (Exception e)
                {
                    Utils.Trace(e, "Error creating the address space.");
                }

                AddPredefinedNode(SystemContext, root);

                // initialize data
                _faultClock.Reset();
                _pressureStableStartTime = DateTime.Now;
                _idealCycleTimeMinimum = IdealCycleTimeDefault / 2;
                _random = new Random();
            }
        }

        /// <summary>
        /// Sets properies of the Execute method.
        /// </summary>
        private void SetExecuteMethodProperties(ref MethodState method)
        {
            // define input arguments
            method.InputArguments = new PropertyState<Argument[]>(method)
            {
                NodeId = new NodeId(method.BrowseName.Name + "InArgs", NamespaceIndex),
                BrowseName = BrowseNames.InputArguments
            };
            method.InputArguments.DisplayName = method.InputArguments.BrowseName.Name;
            method.InputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
            method.InputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            method.InputArguments.DataType = DataTypeIds.Argument;
            method.InputArguments.ValueRank = ValueRanks.OneDimension;

            method.InputArguments.Value = new Argument[]
            {
                new Argument() { Name = "CurrentShift", Description = "The current shift the station is in.",  DataType = DataTypeIds.Int32, ValueRank = ValueRanks.Scalar },
                new Argument() { Name = "ProductSerialNumber", Description = "The serial number of the part to be manufactured.",  DataType = DataTypeIds.UInt64, ValueRank = ValueRanks.Scalar }
            };

            method.OnCallMethod = new GenericMethodCalledEventHandler(OnExecuteCall);
        }

        /// <summary>
        /// Sets properies of the Reset method.
        /// </summary>
        private void SetResetMethodProperties(ref MethodState method)
        {
            method.OnCallMethod = new GenericMethodCalledEventHandler(OnResetCall);
        }

        /// <summary>
        /// Sets properies of the OpenPressureReleaseValve method.
        /// </summary>
        private void SetOpenPressureReleaseValveMethodProperties(ref MethodState method)
        {
            method.OnCallMethod = new GenericMethodCalledEventHandler(OnOpenPressuerReleaseValveCall);
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateBaseVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank, byte accessLevel)
        {
            return CreateBaseVariable(parent, path, name, (uint)dataType, valueRank, accessLevel);
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateBaseVariable(NodeState parent, string path, string name, NodeId dataType, int valueRank, byte accessLevel)
        {
            BaseDataVariableState variable = new BaseDataVariableState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                NodeId = new NodeId(path, NamespaceIndex),
                BrowseName = new QualifiedName(path, NamespaceIndex),
                DisplayName = new LocalizedText("en", name),
                WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                DataType = dataType,
                ValueRank = valueRank,
                AccessLevel = accessLevel,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false,
                Value = TypeInfo.GetDefaultValue(dataType, valueRank, Server.TypeTree),
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow
            };

            if (valueRank == ValueRanks.OneDimension)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            }
            else if (valueRank == ValueRanks.TwoDimensions)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0, 0 });
            }

            parent?.AddChild(variable);

            return variable;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private DataItemState CreateDataItemVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank, byte accessLevel)
        {
            DataItemState variable = new DataItemState(parent);
            variable.ValuePrecision = new PropertyState<double>(variable);
            variable.Definition = new PropertyState<string>(variable);

            variable.Create(
                SystemContext,
                null,
                variable.BrowseName,
                null,
                true);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.NodeId = new NodeId(path, NamespaceIndex);
            variable.BrowseName = new QualifiedName(path, NamespaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.None;
            variable.UserWriteMask = AttributeWriteMask.None;
            variable.DataType = (uint)dataType;
            variable.ValueRank = valueRank;
            variable.AccessLevel = accessLevel;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.Value = TypeInfo.GetDefaultValue((uint)dataType, valueRank, Server.TypeTree);
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;

            if (valueRank == ValueRanks.OneDimension)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            }
            else if (valueRank == ValueRanks.TwoDimensions)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0, 0 });
            }

            variable.ValuePrecision.Value = 2;
            variable.ValuePrecision.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.ValuePrecision.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Definition.Value = String.Empty;
            variable.Definition.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Definition.UserAccessLevel = AccessLevels.CurrentReadOrWrite;

            parent?.AddChild(variable);

            return variable;
        }

        /// <summary>
        /// Creates a new variable using type Numeric as NodeId.
        /// </summary>
        private DataItemState CreateDataItemVariable(NodeState parent, uint id, string name, BuiltInType dataType, int valueRank, byte accessLevel)
        {
            DataItemState variable = new DataItemState(parent);
            variable.ValuePrecision = new PropertyState<double>(variable);
            variable.Definition = new PropertyState<string>(variable);

            variable.Create(
                SystemContext,
                null,
                variable.BrowseName,
                null,
                true);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.NodeId = new NodeId(id, NamespaceIndex);
            variable.BrowseName = new QualifiedName(name, NamespaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.None;
            variable.UserWriteMask = AttributeWriteMask.None;
            variable.DataType = (uint)dataType;
            variable.ValueRank = valueRank;
            variable.AccessLevel = accessLevel;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.Value = Opc.Ua.TypeInfo.GetDefaultValue((uint)dataType, valueRank, Server.TypeTree);
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;

            if (valueRank == ValueRanks.OneDimension)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            }
            else if (valueRank == ValueRanks.TwoDimensions)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0, 0 });
            }

            variable.ValuePrecision.Value = 2;
            variable.ValuePrecision.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.ValuePrecision.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Definition.Value = String.Empty;
            variable.Definition.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Definition.UserAccessLevel = AccessLevels.CurrentReadOrWrite;

            parent?.AddChild(variable);

            return variable;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateVariable(NodeState parent, string path, string name, NodeId dataType, int valueRank)
        {
            BaseDataVariableState variable = new BaseDataVariableState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                NodeId = new NodeId(path, NamespaceIndex),
                BrowseName = new QualifiedName(path, NamespaceIndex),
                DisplayName = new LocalizedText("en", name),
                WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                DataType = dataType,
                ValueRank = valueRank,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false,
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow
            };

            if (valueRank == ValueRanks.OneDimension)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            }
            else if (valueRank == ValueRanks.TwoDimensions)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0, 0 });
            }

            parent?.AddChild(variable);

            return variable;
        }

        /// <summary>
        /// Creates a new method.
        /// </summary>
        private MethodState CreateMethod(NodeState parent, string path, string name)
        {
            MethodState method = new MethodState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                NodeId = new NodeId(path, NamespaceIndex),
                BrowseName = new QualifiedName(path, NamespaceIndex),
                DisplayName = new LocalizedText("en", name),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                Executable = true,
                UserExecutable = true
            };

            parent?.AddChild(method);

            return method;
        }

        /// <summary>
        /// Creates a new method using type Numeric for the NodeId.
        /// </summary>
        private MethodState CreateMethod(NodeState parent, uint id, string name)
        {
            MethodState method = new MethodState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                NodeId = new NodeId(id, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = new LocalizedText("en", name),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                Executable = true,
                UserExecutable = true
            };

            parent?.AddChild(method);

            return method;
        }

        /// <summary>
        /// Method to start production of a part with the given serial number. Executes synchronously.
        /// </summary>
        private ServiceResult OnExecuteCall(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            int NewShift = (int)inputArguments[0];
            ProductSerialNumber = (ulong)inputArguments[1];
            if (NewShift != CurrentShift)
            {
                NumberOfDiscardedProductsInShift = 0;
                NumberOfManufacturedProductsInShift = 0;
                CurrentShift = NewShift;
            }
            _cycleStartTime = DateTime.Now;
            StationState = StationStatus.WorkInProgress;

            ulong idealCycleTime = IdealCycleTime;
            if (idealCycleTime < _idealCycleTimeMinimum)
            {
                IdealCycleTime = idealCycleTime = _idealCycleTimeMinimum;
            }
            int cycleTime = (int)(idealCycleTime + Convert.ToUInt32(Math.Abs((double)idealCycleTime * NormalDistribution(_random, 0.0, 0.1))));

            bool stationFailure = (NormalDistribution(_random, 0.0, 1.0) > 3.0);
            if (stationFailure)
            {
                // the simulated cycle will take longer when the station fails
                cycleTime = FAILURE_CYCLE_TIME + Convert.ToInt32(Math.Abs((double)FAILURE_CYCLE_TIME * NormalDistribution(_random, 0.0, 1.0)));
                NumberOfFaults++;
                NumberOfFaultsInShift++;
                Logger.Information($"Station is in fault for {cycleTime} msec.");
                Logger.Information($"Overall number of faults: {NumberOfFaults}");
                Logger.Information($"Number of faults in shift: {NumberOfFaultsInShift}");
            }

            _simulationTimer = new Timer(SimulationFinished, stationFailure, cycleTime, Timeout.Infinite);
            UpdateFaultyTime();

            Logger.Debug($"Execute method called. Now building product #{ProductSerialNumber} in shift {CurrentShift}");
            return ServiceResult.Good;
        }

            /// <summary>
            /// Method to reset the station. Executes synchronously.
            /// </summary>
            private ServiceResult OnResetCall(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            _faultClock.Stop();
            StationState = StationStatus.Ready;
            UpdateFaultyTime();

            Logger.Debug("Reset method called");
            return ServiceResult.Good;
        }

        /// <summary>
        /// Method to open the pressure release valve. Executes synchronously.
        /// </summary>
        private ServiceResult OnOpenPressuerReleaseValveCall(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            Pressure = PRESSURE_DEFAULT;
            _pressureStableStartTime = DateTime.Now;
            Logger.Debug("Reset method called");
            return ServiceResult.Good;
        }

        private void UpdateFaultyTime()
        {
            if (!_faultClock.IsRunning)
            {
                FaultyTime = (ulong)_faultClock.ElapsedMilliseconds;
                if (_faultClock.ElapsedMilliseconds != 0)
                {
                    _faultClock.Reset();
                }
            }
        }

        private double NormalDistribution(Random rand, double mean, double stdDev)
        {
            // it's possible to convert a generic normal distribution function f(x) to a standard
            // normal distribution (a normal distribution with mean=0 and stdDev=1) with the
            // following formula:
            //
            //  z = (x - mean) / stdDev
            //
            // then with z value you can retrieve the probability value P(X>x) from the standard
            // normal distribution table

            // these are uniform(0,1) random doubles
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();

            // random normal(0,1)
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            // random normal(mean,stdDev^2)
            //Logger.Debug($"NormalDistribution: mean: {mean}, stdDev: {stdDev},  randStdNormal: {randStdNormal}, return: {mean + stdDev * randStdNormal}, u1: {u1}, u2: {u2},");
            return mean + (stdDev * randStdNormal);
        }

        private void SimulationFinished(object state)
        {
            CalculateSimulationResult((bool)state);
            UpdateFaultyTime();
            _simulationTimer.Dispose();
            _simulationTimer = null;
        }

        private void StopPressureHighPhase(object unused)
        {
            Logger.Debug("Stop pressure high phase");
            Pressure = PRESSURE_DEFAULT;
            _pressureStableStartTime = DateTime.Now;
            _pressureHighTimer.Dispose();
            _pressureHighTimer = null;
        }

        public virtual void CalculateSimulationResult(bool stationFailure)
        {
            bool productDiscarded = (NormalDistribution(_random, 0.0, 1.0) > 2.0);

            if (stationFailure)
            {
                NumberOfDiscardedProducts++;
                StationState = StationStatus.Fault;
                _faultClock.Start();
            }
            else if (productDiscarded)
            {
                StationState = StationStatus.Discarded;
                NumberOfDiscardedProducts++;
                NumberOfDiscardedProductsInShift++;
            }
            else
            {
                StationState = StationStatus.Done;
                NumberOfManufacturedProducts++;
                NumberOfManufacturedProductsInShift++;
            }
            Logger.Debug($"Station status is '{StationState}'");
            Logger.Debug($"Overall number of parts: {NumberOfManufacturedProducts}");
            Logger.Debug($"Overall number discarded parts: {NumberOfDiscardedProducts}");
            Logger.Debug($"Number of parts in shift: {NumberOfManufacturedProductsInShift}");
            Logger.Debug($"Number of discarded parts in shift: {NumberOfDiscardedProductsInShift}");

            ActualCycleTime = (ulong)(DateTime.Now - _cycleStartTime).TotalMilliseconds;
            Logger.Debug($"Actual cycle time is {ActualCycleTime}");

            // The power consumption of the station increases exponentially if the ideal cycle time is reduced below the default ideal cycle time
            double idealCycleTime = IdealCycleTime;
            double cycleTimeModifier = (1 / Math.E) * (1 / Math.Exp(-(double)IdealCycleTimeDefault / idealCycleTime));
            Logger.Debug($"Cycle time modifier is {cycleTimeModifier}");
            Logger.Debug($"Ideal cycle time default is {IdealCycleTimeDefault}");
            Logger.Debug($"Ideal cycle time is {idealCycleTime}");
            _powerConsumptionAdjusted = PowerConsumption * cycleTimeModifier;
            Logger.Debug($"Power consumption is {PowerConsumption}");
            Logger.Debug($"Adjusted power consumption is {_powerConsumptionAdjusted}");

            // assume the station consumes only power during the active cycle
            // energy consumption [kWh] = (PowerConsumption [kW] * actualCycleTime [s]) / 3600
            EnergyConsumption = (_powerConsumptionAdjusted * (ActualCycleTime / 1000.0)) / 3600.0;
            Logger.Debug($"New energy consumption is {EnergyConsumption}");

            // For stations configured to generate alerts, calculate pressure
            // Pressure will be stable for PRESSURE_STABLE_TIME and then will increase to PRESSURE_HIGH and stay there PRESSURE_HIGH_TIME or until OpenPressureReleaseValve() is called
            double normalDist = NormalDistribution(_random, (cycleTimeModifier - 1.0) * 100.0, 50.0);
            if (GenerateAlerts && ((DateTime.Now - _pressureStableStartTime).TotalMilliseconds) > PRESSURE_STABLE_TIME)
            {
                // slowly increase pressure until PRESSURE_HIGH is reached
                Logger.Debug($"Current pressure is {Pressure}");
                Pressure += Math.Abs(normalDist);
                Logger.Debug($"New pressure is {Pressure} using {Math.Abs(normalDist)}");

                if (Pressure <= PRESSURE_DEFAULT)
                {
                    Pressure = PRESSURE_DEFAULT + normalDist;
                    Logger.Debug($"Pressure is below default ({PRESSURE_DEFAULT}). Now set to {Pressure} using {normalDist}");
                }
                if (Pressure >= PRESSURE_HIGH)
                {
                    if (_pressureHighTimer == null && PRESSURE_HIGH_TIME != 0)
                    {
                        Logger.Debug("--> Starting Pressure high timer.");
                        _pressureHighTimer = new Timer(StopPressureHighPhase, null, (int)PRESSURE_HIGH_TIME, Timeout.Infinite);
                    }
                    Pressure = PRESSURE_HIGH + normalDist;
                    Logger.Debug($"Pressure above max ({PRESSURE_HIGH}). Now set to {Pressure} using {normalDist}");
                }
            }
            else
            {
                Pressure += normalDist;
                Logger.Debug($"New pressure is {Pressure} using {normalDist}");
            }
        }

        private const int FAILURE_CYCLE_TIME = 5000;            // in ms
        private const ulong PRESSURE_STABLE_TIME = 60 * 1000;   // in ms
        private const ulong PRESSURE_HIGH_TIME = 120 * 1000;     // in ms
        private const double PRESSURE_DEFAULT = 2500;           // in mbar
        private const double PRESSURE_HIGH = 6000;              // in mbar
        private const double POWERCONSUMPTION_DEFAULT = 150;    // in kW
        private const ulong IDEAL_CYCLETIME_DEFAULT = 7 * 1000; // in ms
        private BaseDataVariableState _currentShift;
        private BaseDataVariableState _productSerialNumber;
        private BaseDataVariableState _numberOfManufacturedProducts;
        private BaseDataVariableState _numberOfDiscardedProducts;
        private BaseDataVariableState _numberOfManufacturedProductsInShift;
        private BaseDataVariableState _numberOfDiscardedProductsInShift;
        private BaseDataVariableState _overallRunningTime;
        private BaseDataVariableState _faultyTime;
        private BaseDataVariableState _numberOfFaults;
        private BaseDataVariableState _numberOfFaultsInShift;
        private BaseDataVariableState _stationState;
        private BaseDataVariableState _energyConsumption;        // in kWh
        private BaseDataVariableState _pressure;                 // in mbar
        private BaseDataVariableState _idealCycleTime;           // in ms
        private BaseDataVariableState _actualCycleTime;          // in ms
        private DateTime _pressureStableStartTime;
        private readonly Stopwatch _faultClock = new Stopwatch();
        private DateTime _cycleStartTime;
        private ulong _idealCycleTimeMinimum;
        private Timer _simulationTimer = null;
        private Timer _pressureHighTimer = null;
        private Random _random;
        private double _powerConsumptionAdjusted;
    }
}