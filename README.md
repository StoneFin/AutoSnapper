AutoSnapper
===========

Automated Amazon Web Services Snapshot Management Utility

.NET project built in Visual Studio 2013.

This is a quick console app we put together that automates a few processes for Amazon Web Services.

It can make snapshots of your Volumes and purge aging snapshots.

It can start EC2 instances and associate them with an ElasticIP.

It can stop EC2 instances.

How to Use the Utility
===========

Install the utility on any machine, a production server that runs 24/7 is recommended.

Set up scheduled tasks with the proper command line arg to execute each job at the desired time.

Command Line Arguments
===========

/snapshotVolumes: Use this to snapshot your AWS volumes and delete any snapshots that are older than what is specified in SnapshotExpiration.

/startInstances: Use this to start the instances listed in the App.config InstancesToStart node and associate them with their corresponding ElasticIP.

/stopInstances: Use this to stop the instances listed in the App.config InstancesToStop node.

App.config Settings
===========

The following keys are supported in the App.config:

AWSAccessKey: Your AWS Access Key

AWSSecretKey: Your AWS Secret Access Key

AWSRegion: Your AWS Region

SnapshotExpiration: Snapshot Expiration, in Days. Remove or set to 0 to never delete old snapshots.

SnapshotOwnerId: The OwnerId for your snapshots. "self" means your AWS account.

SnapshotTagKey, SnapshotTagValue: The tag that the automated snapshots will get. This is what newly created snapshots will be tagged with, and the tag the app uses to delete old Snapshots.

InstancesToStart: The list of EC2 instances and their desired ElasticIPs you wish to start up. Leaving the elasticIp value out simply does not associate your EC2 with an ElasticIP.

InstancesToStop: The list of EC2 instances you wish to stop.
