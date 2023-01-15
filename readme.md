### Usage:
---
This program uses a distribution of screp from  https://github.com/icza

1. Open the app and click on settings and set the folder location for the replays
2. Additionally you can set your name so that:

	a. Statistics can be generated for you (Winrates/APM Graph)

	b. The table view columns are aligned so that you will always be the first column and matchups and wins are correctly using your perspective
	
3. Restart the app

### Notes:
----
Why does Ladder filter not filter ladder matches?

* The ladder game type is no longer used by blizzard. It should still filter replays with this type (if you have any), however.

Why did the program determine the winner wrong?
* There's no way to determine who the winner is in a team game if the replay owner leaves the game during its progress. We can determine that the replay owner won or lost. This means that organic 1v1 matches will have the correct winner.

That being said if you've won a team game and it incorrectly displays your match outcome, contact me and send me the replay.

You can include more names by separating them in commas

### Credits:
---
* Special thanks to [icza](https://github.com/icza/) helping me out with some questions (and of course for screp)
* Unofficial thanks to [SCRChart](https://www.scrchart.com/) for making a very fast program that I could benchmark against
and compare what kind of features would be logical to implement