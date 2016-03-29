================================== LEGEND ===================================
=                                                                           =
=	-- STATE_NAME --                                                        =
=			denotes a State definition                                      =
=                                                                           =
=	-> STATE_NAME                                                           =
=			denotes a transition to a State                                 =
=                                                                           =
=	- action                                                                =
=			denotes an action that happens (unconditionally)                =
=                                                                           =
=	--- conditional event                                                   =
=			denotes a conditional event that acts like a trigger            =
=                                                                           =
=	SPEAKER:\n TEXT                                                         =
=			denotes speech spoken by a speaker                              =
=                                                                           =
=	-thought-                                                               =
=			denotes a thought of the player (displayed similarly to speech) =
=                                                                           =
=	-TIME-                                                                  =
=			denotes the duration for which the next element will be shown   =
=                                                                           =
=	+ TEXT                                                                  =
=			denotes an option in a player choice                            =
=                                                                           =
=	if, else, not, var++, var--, var=val, ...                               =
=			are obvious...                                                  =
=                                                                           =
=============================================================================


-- S0 --
	Mother:
		Come, Little Red-Cap, here is a piece of cake and a bottle of wine; take them to your grandmother, she is ill and weak, and they will do her good.
		Set out before it gets hot, and when you are going, walk nicely and quietly and do not run off the path, or you may fall and break the bottle, and then your grandmother will get nothing.
		And when you go into her room, don't forget to say, "Good morning", and don't peep into every corner before you do it.

	Player:
		I will take great care,

	--- waypoint 1
		-> S1


-- S1 --

	- wolf approaches

	Wolf:
		Good day, Little Red-Cap

	Player:
		Thank you kindly, wolf.

	Wolf:
		Whither away so early, Little Red-Cap?

	Player:
		+ To my grandmother's.
			-> S2
		+ To the Huntsman's.
			-> DEAD
				(Your caution roused the wolf and he ate you.)
				[Little Red Ridding Hood \n TIMESTAMP \n Who was cautious perhaps too much.]


-- S2 --

	Wolf:
		What have you got in that basket?

	Player:
		Cake and wine; yesterday was baking-day, so poor sick grandmother is to have something good, to make her stronger.

	Wolf:
		It does smell delicious.

	Player:
		+ Thank you.
			-> S4
		+ Thank you! Would you like some?
			- friendship++
			-> S3


-- S3 --

	Wolf:
		-0.5 sec-
		I would love a bite of you
		I would love a bite of your cake

	Player:
		Here is some.

	-> S4


-- S4 --

	Wolf:
		Where does your grandmother live, Little Red-Cap?

	Player:
		+ Just past the Huntsman's house.
			- friendship--
			-> S5
		+ A good quarter of a league farther on in the wood.
			-> S5
		+ A good quarter of a league farther on in the wood. Her house stands under the three large oak-trees, the nut-trees are just below; you surely must know it.
			- friendship++
			-> S5


-- S5 --

	- if friendship < 0
		-> DEAD
			(Your caution roused the wolf and he ate you.)
			[Little Red Ridding Hood \n TIMESTAMP \n Who was a bit too cautious.]
	- if friendship == 0
		-> SAmbush
	- if friendship > 0
		-> S6


-- SAmbush --

	- wolf hides in trees
	- red eyes are visible
	- trigger spawns
	
	--- on trigger enter
		wolf emerges and eats you
		-> DEAD
			(The wolf ambushed you and you got eaten.)
			[Little Red Ridding Hood \n TIMESTAMP \n Who forgot her detrimental prudence at her grandmother's.]


-- S6 --

	- wolf walks with you

	--- near flower field
		-> S7


-- S7 --

	- if friendship == 1
		-> S8
	- if friendship > 1
		-> SF8


-- S8 --

	Wolf:
		See, Little Red-Cap, how pretty the flowers are about here — why do you not look round?
		I believe, too, that you do not hear how sweetly the little birds are singing.
		You walk gravely along as if you were going to school, while everything else out here in the wood is merry.

	Player: 
		-thought-
		Suppose I take grandmother a fresh nosegay; that would please her too. It is so early in the day that I shall still get there in good time.

	--- enter flower field
		-> S9

	--- go towards bridge
		-> S10


-- S9 --

	- flower collecting game

	--- wolf gets off screen
		(trigger at a measured distance)
		-> S10


-- S10 --

	- wolf runs into the woods

	--- sees broken bridge
		Player:
			-thought-
			The bridge is broken. I will have to find another way.

	--- goes to cemetery
		Player:
			-thought-
			This seems to be a cemetery.

	--- gets close to a grave
		- speech with the engraving

	--- goes to grandma's
		-> DEAD
			(-Y-o-u-r- -G-r-a-n-d-m-a- The wolf \n ate you)
			[Little Red Riding Hood \n TIMESTAMP \n Who naively walked through the woods.]
			[Granny \n TIMESTAMP \n Who became the treat for her guest the wolf.]



-- SF8 --

	Wolf:
		See, Little Red-Cap, how pretty the flowers are about here — would their freshness not please your grandmother?
		I dare say, too, that I can gather the most refreshing of them all, before you do.

	Player: 
		-thought-
		Suppose I take grandmother a fresh nosegay; that would please her too. It is so early in the day that I shall still get there in good time.

	- flower collecting game; wolf plays too

	--- you win the dare
		- has_reason_to_thank_wolf = false

	--- wolf wins the dare
		- has_reason_to_thank_wolf = true

	-> SF9


-- SF9 --

	- The Huntsman approaches

	Huntsman:
		Dear girl, that there is the cunning wolf!
		Leave his side and come behind me!

	--- cross a threshold === choose Huntsman
		-> SFH10

	--- timed event === stay by wolf
		-> SFW10


-- SFH10 --

	Wolf:
		How can I allow such a plump mouthful to escape!

	- wolf goes after you
	- the Huntsman rushes towards the wolf too

	--- drop / throw the basket
		-> SFH11

	--- run with the basket
		-> SFH12


-- SFH11 --

	- basket fades out (breaks)
	- you manage to get behind the huntsman
	- the huntsman kills the wolf (wolf fades out)

	Huntsman:
		Let's get you home, little dear.

	-> END
		(You were brought home safely. \n But the wolf died and your grandmother did not receive anything.)


	-- SFH12 --

	- the wolf catches you (you fade out)

	-> END
		(You were eaten by the wolf.)
		[Little Red Ridding Hood \n TIMESTAMP \n Who held a basket more important than her life.]



-- SFW10 --

	Player:
		Leave this kind creature alone!

	Huntsman:
		Silly Girl...

	- Huntsman starts running away

	Huntsman:
		But I will abide.

	- huntsman escapes

	-> SFW11


-- SFW11 --

	Wolf:
		Let me escort you to your grandma's, dear girl.

	-> SFW11A


-- SFW11A --

	- wolf walks with you

	--- get to the broken bridge
		- if not shortcut_used
			-> SFW11B

	--- get on the path to grandmother's
		-> SFW12


-- SFW11B --

	Wolf:
		The bridge seems broken, but I know another way. Follow me.

	--- get to the wolf by the path closeby
		- shortcut_used = true
		- has_reason_to_thank_wolf = true
		-> SFW11A


-- SFW12 --

	- if has_reason_to_thank_wolf
		-> SFWT13
	- else
		-> SFW13


-- SFW13 --

	Wolf:
		I shall leave you now, dear. Goodbye!

	Player:
		Goodbye

	-> END
		(Your day goes by peacefully. \n But your grandma gets eaten the next day.)
		[Grandmother \n TIMESTAMP \n Who became the treat to her new neighbour the wolf.]


-- SFWT13 --

	Wolf:
		I shall leave you now, dear. Goodbye!

	Player:
		Allow me to thank you for the aid you have given me!
		Let me treat you to some wine.

	Wolf:
		How lovely...

	- beat

	- wolf is drunk and sleeping

	- wolf_dragged_away = false

	--- drags wolf away (past threshold)
		- wolf_dragged_away = true

	--- enters grandma's house
		-> SFWT14


-- SFWT14 --
	
	- if wolf_dragged_away
		-> END
			(Your day goes by peacefully. \n You managed to save everyone from an ill fate.)
			(The wolf woke up hungover and did not remember anything.)
			(The huntsman was alive and well and paid you a visit the next day.)
			(Your grandmother was cheered up by the nosegay you gave her. She also soon got better because of the treats you brought her.)

	- else
		-> END
			(Your day goes by peacefully. \n But your grandma gets eaten the next day.)
			[Grandmother \n TIMESTAMP \n Who became the treat to her newest visitor the wolf.]
	