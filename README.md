AutoSnapper
===========

Automated Amazon Web Services Snapshot Management Utility

This is a quick console app we put together that automates the Snapshot process for Amazon Web Services.

It makes snapshots of AWS volumes deletes aging snapshots, according to your App.config settings.

See App.example.config for a sample.

The following keys are supported in the App.config:

AWSAccessKey: Your AWS Access Key

AWSSecretKey: Your AWS Secret Access Key

AWSRegion: Your AWS Region

SnapshotExpiration: Snapshot Expiration, in Days. Remove or set to 0 to never delete old snapshots.

SnapshotOwnerId: The OwnerId for your snapshots. "self" means your AWS account.

SnapshotTagKey, SnapshotTagValue: The tag that the automated snapshots will get. This is what newly created snapshots will be tagged with, and the tag the app uses to delete old Snapshots.
