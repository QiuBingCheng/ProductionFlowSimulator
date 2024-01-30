NumberOfServiceNodes:3
Name: Washing Workstation
HashCode: 13213278
Bound: 80 60 490 155
BackColor: -32
NumberofServers:1
NumberofQueues:1
Type Information:DiscreteEventSimulationLibrary.Machine,DiscreteEventSimulationLibrary
Name: W
HashCode: 44307222
Bound: 100 101 70 70
BackColor: -16744448
ServiceTimeGeneratorType: 3
Mean: 1
Sigma: 0.5
LowerBound: 0.1
UpperBound: 5
NumberofServerQueues: 1
Name: Qw
HashCode: 63220684
Bound: 230 100 300 60
BackColor: -1
Capacity: 10
IsPrioritized: False
EnableBreakAndRepair: False
BreakDownTimeGeneratorType: 0
RepairTimeGeneratorType: 0
Name: Press Workstation
HashCode: 51810644
Bound: 80 245 490 270
BackColor: -32
NumberofServers:3
NumberofQueues:3
Type Information:DiscreteEventSimulationLibrary.Machine,DiscreteEventSimulationLibrary
Name: P1
HashCode: 32115247
Bound: 100 279 60 60
BackColor: -16744448
ServiceTimeGeneratorType: 2
Lower Bound: 0
Upper Bound: 1
NumberofServerQueues: 1
Name: Qp1
HashCode: 20601768
Bound: 230 287 280 54
BackColor: -1
Capacity: 8
IsPrioritized: False
EnableBreakAndRepair: True
BreakDownTimeGeneratorType: 1
Mean: 500
RepairTimeGeneratorType: 1
Mean: 10
Type Information:DiscreteEventSimulationLibrary.Machine,DiscreteEventSimulationLibrary
Name: P2
HashCode: 51198184
Bound: 100 376 50 50
BackColor: -16744448
ServiceTimeGeneratorType: 2
Lower Bound: 0
Upper Bound: 1
NumberofServerQueues: 1
Name: Qp2
HashCode: 58130472
Bound: 230 371 270 59
BackColor: -1
Capacity: 7
IsPrioritized: False
EnableBreakAndRepair: False
BreakDownTimeGeneratorType: 0
RepairTimeGeneratorType: 0
Type Information:DiscreteEventSimulationLibrary.Machine,DiscreteEventSimulationLibrary
Name: P3
HashCode: 53412201
Bound: 100 453 52 52
BackColor: -16744448
ServiceTimeGeneratorType: 2
Lower Bound: 0
Upper Bound: 1
NumberofServerQueues: 1
Name: Qp3
HashCode: 10947764
Bound: 230 454 260 51
BackColor: -1
Capacity: 6
IsPrioritized: False
EnableBreakAndRepair: False
BreakDownTimeGeneratorType: 0
RepairTimeGeneratorType: 0
Name: Assembly Workstation
HashCode: 63642613
Bound: 80 545 490 155
BackColor: -32
NumberofServers:2
NumberofQueues:1
Type Information:DiscreteEventSimulationLibrary.Machine,DiscreteEventSimulationLibrary
Name: Y1
HashCode: 31421019
Bound: 100 568 50 50
BackColor: -16744448
ServiceTimeGeneratorType: 2
Lower Bound: 0
Upper Bound: 1
NumberofServerQueues: 1
Name: Qy
HashCode: 23240469
Bound: 230 583 290 70
BackColor: -1
Capacity: 9
IsPrioritized: True
EnableBreakAndRepair: True
BreakDownTimeGeneratorType: 1
Mean: 500
RepairTimeGeneratorType: 1
Mean: 10
Type Information:DiscreteEventSimulationLibrary.Machine,DiscreteEventSimulationLibrary
Name: Y2
HashCode: 14353717
Bound: 100 640 44 44
BackColor: -16744448
ServiceTimeGeneratorType: 2
Lower Bound: 0
Upper Bound: 1
NumberofServerQueues: 1
Name: Qy
HashCode: 23240469
Bound: 230 583 290 70
BackColor: -1
Capacity: 9
IsPrioritized: True
EnableBreakAndRepair: False
BreakDownTimeGeneratorType: 0
RepairTimeGeneratorType: 0
NumberOfClientGenerator:1
Name: Releaser1
HashCode: 62074597
Bound: 766 122 91 91
BackColor: -40121
ClientArrivalTimeGeneratorType: 1
Mean: 2
NumberOfItineraries: 2
GenerationWeights: 70,30
Name: PartA
HashCode: 21800467
Bound: 662 88 70 70
BackColor: -5383962
Priority Weight:1
NumberofVisits: 3
Name: Generator1
HashCode: 61986480
Bound: 578 155 26 26
BackColor: -5383962
TheNodeHashCode:13213278
ServiceTimeGeneratorType: 0
Name: PartAPressTime
HashCode: 21007413
Bound: 578 398 26 26
BackColor: -5383962
TheNodeHashCode:51810644
ServiceTimeGeneratorType: 1
Mean: 8
Name: PartAAssTime
HashCode: 54848996
Bound: 578 640 26 26
BackColor: -5383962
TheNodeHashCode:63642613
ServiceTimeGeneratorType: 1
Mean: 5.5
Name: PartB
HashCode: 23878916
Bound: 669 187 67 67
BackColor: -5383962
Priority Weight:2
NumberofVisits: 3
Name: Generator2
HashCode: 13583655
Bound: 578 112 24 24
BackColor: -5383962
TheNodeHashCode:13213278
ServiceTimeGeneratorType: 0
Name: PartBPressTime
HashCode: 55144039
Bound: 578 355 24 24
BackColor: -5383962
TheNodeHashCode:51810644
ServiceTimeGeneratorType: 1
Mean: 6
Name: PartBAssTime
HashCode: 26534308
Bound: 578 597 24 24
BackColor: -5383962
TheNodeHashCode:63642613
ServiceTimeGeneratorType: 1
Mean: 4.5
