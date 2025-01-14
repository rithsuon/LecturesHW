//Group: Rith Suon and Nicholas Bautista
/// Card representations.
// An "enum"-type union for card suit.
type CardSuit = 
    | Spades 
    | Clubs
    | Diamonds
    | Hearts

// Kinds: 1 = Ace, 2 = Two, ..., 11 = Jack, 12 = Queen, 13 = King.
type Card = {suit : CardSuit; kind : int}


/// Game state records.
// One hand being played by the player: its cards, and a flag for whether it was doubled-down.
type PlayerHand = {
    cards: Card list; 
    doubled: bool
}

// All the hands being played by the player: the hands that are still being played (in the order the player must play them),
// and the hands that have been finished (stand or bust).
type PlayerState = {
    activeHands: PlayerHand list; 
    finishedHands: PlayerHand list
}

// The state of a single game of blackjack. Tracks the current deck, the player's hands, and the dealer's hand.
type GameState = {
    deck : Card list; 
    player : PlayerState; 
    dealer: Card list
}

// A log of results from many games of blackjack.
type GameLog = {playerWins : int; dealerWins : int; draws : int}

/// Miscellaneous enums.
// Identifies whether the player or dealer is making some action.
type HandOwner = 
    | Player 
    | Dealer

// The different actions a player can take.
type PlayerAction = 
    | Hit
    | Stand
    | DoubleDown
    | Split

// The result of one hand that was played.
type HandResult = 
    | Win
    | Lose
    | Draw


// This global value can be used as a source of random integers by writing
// "rand.Next(i)", where i is the upper bound (exclusive) of the random range.
let rand = new System.Random()


// UTILITY METHODS

// Returns a string describing a card.
let cardToString card =
    // Done: replace the following line with logic that converts the card's kind to a string.
    // Reminder: a 1 means "Ace", 11 means "Jack", 12 means "Queen", 13 means "King".
    // A "match" statement will be necessary. (The next function below is a hint.)
    let kind = 
        match card.kind with
        |1 -> "Ace"
        |11-> "Jack"
        |12-> "Queen"
        |13->"King"
        |_-> string card.kind

    // "%A" can print any kind of object, and automatically converts a union (like CardSuit)
    // into a simple string.
    sprintf "%s of %A" kind card.suit


// Returns a string describing the cards in a hand.    
let handToString hand =
    // Done: replace the following line with statement(s) to build a string describing the given hand.
    // The string consists of the results of cardToString when called on each Card in the hand (a Card list),
    // separated by commas. You need to build this string yourself; the built-in "toString" methods for lists
    // insert semicolons and square brackets that I do not want.
    hand |> List.map cardToString |> String.concat ", " 
    
    // Hint: transform each card in the hand to its cardToString representation. Then read the documentation
    // on String.concat.

    
// Returns the "value" of a card in a poker hand, where all three "face" cards are worth 10
// and an Ace has a value of 11.
let cardValue card =
    match card.kind with
    | 1 -> 11
    | 11 | 12 | 13 -> 10  // This matches 11, 12, or 13.
    | n -> n
    
    // Reminder: the result of the match will be returned


// Calculates the total point value of the given hand (Card list). 
// Find the sum of the card values of each card in the hand. If that sum
// exceeds 21, and the hand has aces, then some of those aces turn from 
// a value of 11 to a value of 1, and a new total is computed.
// TODO: fill in the marked parts of this function.
let handTotal hand =
    // Done: modify the next line to calculate the sum of the card values of each
    // card in the list. Hint: List.map and List.sum. (Or, if you're slick, List.sumBy)
    let sum = hand |> List.map (fun x -> cardValue x) |> List.sum 

    // Done: modify the next line to count the number of aces in the hand.
    // Hint: List.filter and List.length. 
    let numAces = hand |> List.filter (fun c -> c.kind = 1) |> List.length

    // Adjust the sum if it exceeds 21 and there are aces.
    if sum <= 21 then
        // No adjustment necessary.
        sum
    else 
        // Find the max number of aces to use as 1 point instead of 11.
        let maxAces = (float sum - 21.0) / 10.0 |> ceil |> int
        // Remove 10 points per ace, depending on how many are needed.
        sum - (10 * (min numAces maxAces))


// FUNCTIONS THAT CREATE OR UPDATE GAME STATES

// Creates a new, unshuffled deck of 52 cards.
// A function with no parameters is indicated by () in the parameter list. It is also invoked
// with () as the argument.
let makeDeck () =
    // Make a deck by calling this anonymous function 52 times, each time incrementing
    // the parameter 'i' by 1.
    // The Suit of a card is found by dividing i by 13, so the first 13 cards are Spades.
    // The Kind of a card is the modulo of (i+1) and 13. 
    List.init 52 (fun i -> let s = match i / 13 with
                                   | 0 -> Spades
                                   | 1 -> Clubs
                                   | 2 -> Diamonds
                                   | 3 -> Hearts
                           {suit = s; kind = i % 13 + 1})


// Shuffles a list by converting it to an array, doing an in-place Fisher-Yates 
// shuffle, then converting back to a list.
// Don't worry about this.
let shuffleDeck deck =
    let arr = List.toArray deck

    let swap (a: _[]) x y =
        let tmp = a.[x]
        a.[x] <- a.[y]
        a.[y] <- tmp
    
    Array.iteri (fun i _ -> swap arr i (rand.Next(i, Array.length arr))) arr
    Array.toList arr


// Creates a new game state using the given deck, dealing 2 cards to the player and dealer.
let newGame (deck : Card list) =
    // Construct the starting hands for player and dealer.
    let playerCards = [deck.Head ; List.item 2 deck] // First and third cards.
    let dealerCards = [deck.Tail.Head ; List.item 3 deck] // Second and fourth.

    // Return a fresh game state.
    {deck = List.skip 4 deck;
    // the initial player has only one active hand.
     player = {activeHands = [{cards = playerCards; doubled = false}]; finishedHands = []}
     dealer = dealerCards}


// Given a current game state and an indication of which player is "hitting", deal one
// card from the deck and add it to the given person's hand. Return the new game state.
let hit handOwner gameState = 
    let topCard = List.head gameState.deck
    let newDeck = List.tail gameState.deck
    
    // Updating the dealer's hand is easy.
    if handOwner = Dealer then
        let newDealerHand = topCard :: gameState.dealer
        // Return a new game state with the updated deck and dealer hand.
        {gameState with deck = newDeck;
                        dealer = newDealerHand}
    else
        
        // DONE ??: updating the player is trickier. We are always working with the player's first
        // active hand. Create a new first hand by adding the top card to that hand's card list.
        // Then update the player's active hands so that the new first hand is head of the list; and the
        //     other (unchanged) active hands follow it.
        // Then construct the new game state with the updated deck and updated player.
        let newFirstHand = topCard :: gameState.player.activeHands.Head.cards
        let newPlayerHand = {gameState.player.activeHands.Head with cards = newFirstHand}

        let newPlayerState = {gameState.player with activeHands = (newPlayerHand :: List.tail gameState.player.activeHands)}
        {gameState with deck = newDeck; player = newPlayerState}
        // TODO: this is just so the code compiles; fix it.
//move top activeHand to finishedHands
let playerStand (gameState : GameState) =
    let finishedHand = gameState.player.activeHands.Head
    
    let newPlayerState = {gameState.player with finishedHands = (finishedHand :: gameState.player.finishedHands); activeHands = gameState.player.activeHands.Tail}
    {gameState with player = newPlayerState}

//gets the player's top active playerHand and flag it to be doubled
let playerDoubleDown (gameState : GameState) =
    let newPlayerHand = {gameState.player.activeHands.Head with doubled = true}
    let newPlayerState = {gameState.player with activeHands = (newPlayerHand :: List.tail gameState.player.activeHands)}
    hit Player {gameState with player = newPlayerState}


//split two cards from one hand into two hands with one card
let playerSplit (gameState : GameState) =
    let currCardsList = gameState.player.activeHands.Head.cards
    let handOne = {gameState.player.activeHands.Head with cards = currCardsList.Head :: []}
    let handTwo = {gameState.player.activeHands.Head with cards = currCardsList.Tail.Head :: []}

    let newPlayerState = {gameState.player with activeHands = handTwo :: gameState.player.activeHands.Tail}
    let oneDone = hit Player {gameState with player = newPlayerState}
    let secondPlayerState = {oneDone.player with activeHands =  handOne :: oneDone.player.activeHands}
    hit Player {oneDone with player = secondPlayerState}

// Take the dealer's turn by repeatedly taking a single action, hit or stay, until 
// the dealer busts or stays.
let rec dealerTurn gameState =
    let dealer = gameState.dealer
    let score = handTotal dealer

    printfn "Dealer's hand: %s; %d points" (handToString dealer) score
    
    // Dealer rules: must hit if score < 17.
    if score > 21 then
        printfn "Dealer busts!"
        // The game state is unchanged because we did not hit. 
        // The dealer does not get to take another action.
        gameState
    elif score < 17 then
        printfn "Dealer hits"
        // The game state is changed; the result of "hit" is used to build the new state.
        // The dealer gets to take another action using the new state.
        gameState
        |> hit Dealer
        |> dealerTurn
    else
        // The game state is unchanged because we did not hit. 
        // The dealer does not get to take another action.
        printfn "Dealer must stay"
        gameState
        

// Take the player's turn by repeatedly taking a single action until they bust or stay.
let rec playerTurn (playerStrategy : GameState->PlayerAction) (gameState : GameState) =
    // TODO: code this method using dealerTurn as a guide. Follow the same standard
    // of printing output. This function must return the new game state after the player's
    // turn has finished, like dealerTurn.
    

    // Unlike the dealer, the player gets to make choices about whether they will hit or stay.
    // The "elif score < 17" code from dealerTurn is inappropriate; in its place, we will
    // allow a "strategy" to decide whether to hit. A "strategy" is a function that accepts
    // the current game state and returns true if the player should hit, and false otherwise.
    // playerTurn must call that function (the parameter playerStrategy) to decide whether
    // to hit or stay.
    let playerState = gameState.player
    

    if playerState.activeHands.IsEmpty then
        // A player with no active hands cannot take an action.
        gameState
    else

        // The next line is just so the code compiles. Remove it when you code the function.
        // Done: print the player's first active hand. Call the strategy to get a PlayerAction.
        // Create a new game state based on that action. Recurse if the player can take another action 
        // after their chosen one, or return the game state if they cannot.
        let playerHand = gameState.player.activeHands.Head.cards
        let score = handTotal playerHand
        printfn "Player's hand: %s; %d points" (handToString playerHand) score
        if  score > 21 then
            printf "Player busts!\n"
            gameState |> playerStand
        else
            let action = playerStrategy gameState
            match action with
            |Stand -> printf "Player stands.\n"
            |Hit -> printf "Player hits!\n"
            |DoubleDown -> printf "Player doubled down!\n"
            |Split -> printf "Player split the hand!\n"

            match action with
            |Stand -> gameState |> playerStand
            |Hit -> gameState |> hit Player |> playerTurn playerStrategy
            |DoubleDown -> gameState |> playerDoubleDown |> playerTurn (fun _ -> Stand) 
            |Split -> gameState |> playerSplit |> playerTurn playerStrategy
                     
let determineWinner (playerHand : Card list) (dealerHand : Card list) =
    let playerScore = handTotal playerHand
    let dealerScore = handTotal dealerHand

    if playerScore <= 21 && (dealerScore > 21 || playerScore > dealerScore) then
        Win
    else if playerScore = dealerScore then
        Draw
    else
        Lose
// Plays one game with the given player strategy. Returns a GameLog recording the winner of the game.
let oneGame playerStrategy gameState =
    // TODO: print the first card in the dealer's hand to the screen, because the Player can see
    // one card from the dealer's hand in order to make their decisions.
    let dealerScore = handTotal gameState.dealer
    printfn "Dealer is showing: %A; %d points" (cardToString gameState.dealer.Head) (cardValue gameState.dealer.Head)

    if dealerScore = 21 then  
        printfn "Dealer's Hand: %A; %d points" (handToString gameState.dealer) dealerScore
        let pScore = handTotal gameState.player.activeHands.Head.cards
        if pScore = dealerScore then
            printfn "Both players have natural blackjack! Draw."
            {playerWins = 0; dealerWins = 0; draws = 1}
        else
            printfn "Natural blackjack! Dealer wins."
            {playerWins = 0; dealerWins = 1; draws = 0}
    else
        printfn "Player's turn" 
        

    // TODO: play the game! First the player gets their turn. The dealer then takes their turn,
    // using the state of the game after the player's turn finished.

        let rec oneGame' (state : GameState) =
            let afterState = playerTurn playerStrategy state
            match afterState.player.activeHands with
            |[] -> afterState
            |hd :: tl -> oneGame' afterState
            
        let afterPlayer = oneGame' gameState

        
        let finalState = dealerTurn afterPlayer
        let rec accWinners (pHand : PlayerHand list) (dHand : Card list) acc =
            match pHand with
            |[] -> acc
            |hd :: tl -> accWinners tl dHand (match determineWinner hd.cards dHand with |Win -> {acc with playerWins = if hd.doubled then 2 + acc.playerWins else 1 + acc.playerWins} |Draw -> {acc with draws = acc.draws + 1} | Lose -> {acc with dealerWins = if hd.doubled then 2 + acc.dealerWins else 1 + acc.dealerWins})
        

        accWinners finalState.player.finishedHands finalState.dealer {playerWins = 0; dealerWins = 0; draws = 0}

        
    // TODO: determine the winner(s)! For each of the player's hands, determine if that hand is a 
    // win, loss, or draw. Accumulate (!!) the sum total of wins, losses, and draws, accounting for doubled-down
    // hands, which gets 2 wins, 2 losses, or 1 draw
    
    // The player wins a hand if they did not bust (score <= 21) AND EITHER:
    // - the dealer busts; or
    // - player's score > dealer's score
    // If neither side busts and they have the same score, the result is a draw.



// Plays n games using the given playerStrategy, and returns the combined game log.
let manyGames n playerStrategy =
    // TODO: run oneGame with the playerStrategy n times, and accumulate the result. 
    // If you're slick, you won't do any recursion yourself. Instead read about List.init, 
    // and then consider List.reduce.
    let combiner (curr : GameLog) (next : GameLog) =
        {curr with playerWins = curr.playerWins + next.playerWins; dealerWins = curr.dealerWins + next.dealerWins; draws = curr.draws + next.draws}
    List.init n (fun _ -> oneGame playerStrategy (makeDeck() |> shuffleDeck |> newGame)) |> List.reduce combiner 

            

        
// PLAYER STRATEGIES
// Returns a list of legal player actions given their current hand.
let legalPlayerActions playerHand =
    let legalActions = [Hit; Stand; DoubleDown; Split]
    // One boolean entry for each action; True if the corresponding action can be taken at this time.
    let requirements = [
        handTotal playerHand < 21; 
        true; 
        playerHand.Length = 2;
        playerHand.Length = 2 && cardValue playerHand.Head = cardValue playerHand.Tail.Head
    ]

    List.zip legalActions requirements // zip the actions with the boolean results of whether they're legal
    |> List.filter (fun (_, req) -> req) // if req is true, the action can be taken
    |> List.map (fun (act, _) -> act) // return the actions whose req was true


// Get a nice printable string to describe an action.
let actionToString = function
    | Hit -> "(H)it"
    | Stand -> "(S)tand"
    | DoubleDown -> "(D)ouble down"
    | Split -> "S(p)lit"

// This strategy shows a list of actions to the user and then reads their choice from the keyboard.
let rec interactivePlayerStrategy gameState =
    let playerHand = gameState.player.activeHands.Head
    let legalActions = legalPlayerActions playerHand.cards

    legalActions
    |> List.map actionToString
    |> String.concat ", "
    |> printfn "What do you want to do? %s" 

    let answer = System.Console.ReadLine()
    // Return true if they entered "y", false otherwise.
    match answer.ToLower() with
    | "h" when List.contains Hit legalActions -> Hit
    | "s" -> Stand
    | "d" when List.contains DoubleDown legalActions -> DoubleDown
    | "p" when List.contains Split legalActions -> Split
    | _ -> printfn "Please choose one of the available options, dummy."
           interactivePlayerStrategy gameState

let rec inactivePlayerStrategy gameState =
    Stand


let rec greedyPlayerStrategy gameState =
    let playerHand = gameState.player.activeHands.Head
    let legalActions = legalPlayerActions playerHand.cards

    if List.contains Hit legalActions then Hit else Stand 

let rec coinFlipPlayerStrategy gameState = 
    if rand.Next(100) > 49 then
        Hit
    else
        Stand

let basicPlayerStrategy gameState =
    let dFirstCardVal = cardValue gameState.dealer.Head
    let playerHand = gameState.player.activeHands.Head.cards
    let pFirstCardVal = cardValue playerHand.Head
    let pSecCardVal = cardValue playerHand.Tail.Head
    let pScore = handTotal playerHand
    
    if (pFirstCardVal = pSecCardVal) && pFirstCardVal = 5 then
        DoubleDown
    else 
        if pScore = 10 || pScore = 11 || pScore = 9 then
                match pScore with
                |10 when dFirstCardVal = 10 || dFirstCardVal = 11 -> Hit
                |9 when dFirstCardVal = 2 || dFirstCardVal >= 7 -> Hit
                |_ -> DoubleDown
        else if pFirstCardVal = pSecCardVal then
            if pScore = 20 then Stand else Split
        else
            if dFirstCardVal >= 2 && dFirstCardVal <= 6 then
                if pScore >= 12 then Stand else Hit 
            else if dFirstCardVal >= 7 && dFirstCardVal <= 10 then
                if pScore <= 16 then Hit else Stand
            else
                let numAces = playerHand |> List.filter (fun c -> c.kind = 1) |> List.length
                if numAces >= 1 && pScore <= 16 then
                    Hit
                else if pScore <= 11 then 
                    Hit
                else
                Stand


[<EntryPoint>]
let main argv =
    //MyBlackjack.makeDeck() 
    //|> MyBlackjack.shuffleDeck
    //|> MyBlackjack.newGame
    //|> MyBlackjack.oneGame MyBlackjack.recklessPlayer
    //|> printfn "%A"


    manyGames 1000 basicPlayerStrategy |> printfn "%A"
    //manyGames 1000 coinFlipPlayerStrategy
    //|> printfn "%A"
    // TODO: call manyGames to run 1000 games with a particular strategy.

    //testing

    0 // return an integer exit code


