### Game Rules

1. The game is played in real time and can lasts forever in theory. Players can join anytime. 

2. New players are given an initial amount of 1 million coins, with the goal of earning as much as possible throughout the game.

3. The game board is comprised of numerous treasure spots, each of which contains coins and players. The coins on each spot are shared equally among all players on that spot.

4. The only action that can be taken in the game is to move between spots. 

- When you start the game, you are not on any spots, so in your first move, you will bring 1000 coins to the first spot
- In all the subsequent moves, you will bring along your shares coins to the new spot.

5. The game is played in rounds, with an infinite number of rounds in theory. 
 
- Each round lasts for one day, followed by a settlement phase. 
- The settlement will be triggered when this timer goes to zero. 

6. In settlement, all players will collect their share of coins from the treasure spots. 

7. In the beginning of next new round, each players will contribute 1000 coins to the pool, which are distributed evenly among all the treasure spots. 

8. And everyone stay where they are in the new round

9. The number of treasure spots will also be increased logarithmically according to the number of players
- It starts with 2 treasure spots. 
- The number of spots increase by one whenever the number of players doubled. e.g. for 2 players, there are 3 spots, and for 4 players, there are 4 spots, etc.

10. When you make a move, there is a cost to pay
- The cost increases linearly from 0 to 1000 throughout the round (with a minimum of 10)
- i.e. at the beginning of the round, you pay 10 coins to move, whereas in the end, you pay 1000 coins to move.
- The paid cost will be distributed to the treasure spots evently.
