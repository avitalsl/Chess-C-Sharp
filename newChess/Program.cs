using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newChess
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Chess chess = new Chess();
            chess.startGame();
            
         }

        class Chess
        {
            bool isWhiteTurn;
            string invalidDeclaration = "";
            bool isEnPassatThisTurn;
            Piece[,] board = new Piece[8, 8] { { new Rook(0, 0, false),  new Knight(0, 1, false),  new Bishop(0, 2, false),  new Queen(0, 3, false),  new King(0, 4, false),
                    new Bishop(0, 5, false),  new Knight(0, 6, false), new Rook(0, 7, false) }, {new Pawn(1, 0, false),  new Pawn(1, 1, false),  new Pawn(1, 2, false),
                    new Pawn(1, 3, false),  new Pawn(1, 4, false), new Pawn(1, 5, false),  new Pawn(1, 6, false), new Pawn(1, 7, false)}, {new emptyPiece(2, 0),
                    new emptyPiece(2, 1),  new emptyPiece(2, 2),  new emptyPiece(2, 3),  new emptyPiece(2, 4), new emptyPiece(2, 5),  new emptyPiece(2, 6),
                    new emptyPiece(2, 7)}, {new emptyPiece(3, 0),  new emptyPiece(3, 1),  new emptyPiece(3, 2),  new emptyPiece(3, 3),  new emptyPiece(3, 4),
                    new emptyPiece(3, 5),  new emptyPiece(3, 6), new emptyPiece(3, 7)}, {new emptyPiece(4, 0),  new emptyPiece(4, 1),  new emptyPiece(4, 2),
                    new emptyPiece(4, 3),  new emptyPiece(4, 4), new emptyPiece(4, 5),  new emptyPiece(4, 6), new emptyPiece(4, 7)}, {new emptyPiece(5, 0),
                    new emptyPiece(5, 1),  new emptyPiece(5, 2),  new emptyPiece(5, 3),  new emptyPiece(5, 4), new emptyPiece(5, 5),  new emptyPiece(5, 6),
                    new emptyPiece(5, 7)}, {new Pawn(6, 0, true),  new Pawn(6, 1, true),  new Pawn(6, 2, true),  new Pawn(6, 3, true),  new Pawn(6, 4, true),
                    new Pawn(6, 5, true),  new Pawn(6, 6, true), new Pawn(6, 7, true)}, {new Rook(7, 0, true),  new Knight(7, 1, true),  new Bishop(7, 2, true),
                    new Queen(7, 3, true),  new King(7, 4, true), new Bishop(7, 5, true),  new Knight(7, 6, true), new Rook(7, 7, true)}};
            King referenceToWhiteKing;
            King referenceToBlackKing;
            Piece[,] boardCopy = new Piece[8,8];
            Piece[][,] boardHistory = new Piece[50][,];
            int historyIndex = 0;
            int currentRowCopy;
            int currentColumnCopy;
            Piece pieceCaptured;
            int fiftyMovesCount = 0;
            string checkDeclaration = "";
            bool playerChooseToPerformBigCastling = false;
            bool playerChooseToPerformSmallCastling = false;

            public Chess()
            {
                isWhiteTurn = true;               
                referenceToWhiteKing = (King)board[7, 4];
                referenceToBlackKing = (King)board[0, 4];
            }
            public void startGame()
            {
                printBoard();
                Console.WriteLine("Welcome!\nPlayer makes his move by entering:\n 1. current position of the piece he wants to play\n" +
                    " 2. 'to'\n 3. the position he wants to move it to\nThe move should be in the folowing format: char# to char#," +
                    " example: d7 to d5.\nFor castling enter: big/small castling. \nWhite play first, engoy!\n");
                playTurn();
            }
            public void playTurn()
            {
                while (true)
                {
                    string[] move = getValidInput();
                    Piece current = convertInputIntoPiece(move[0]);
                    currentRowCopy = current.row;
                    currentColumnCopy = current.column;
                    Piece destination = convertInputIntoPiece(move[2]);
                    if (!isMoveValid(destination, current))
                    {
                        Console.WriteLine(invalidDeclaration);
                        continue;
                    }
                    if (!doesMoveSavesKing(destination, current))
                    {
                        Console.WriteLine("You have to save your king");
                        continue;
                    }
                    if (!isEnPassatThisTurn)
                        movePiece(destination, current);
                    else
                        performEnPassat(destination, current);
                    if (isCheck())
                        checkDeclaration = "CHECK";
                    if (isCheckMate())
                    {
                        printBoard();
                        Console.WriteLine("CHECKMATE!\n" + (isWhiteTurn? "White":"Black") + " won the game");
                        return;
                    }
                    if (isDraw())
                        return;
                    printBoard();
                    printDeclaration();
                    resetProperties(destination, current);  
                }                                          
            }
            public void printBoard()
            {
                boardCopy = copyBoard(board);
                Console.Clear();
                Console.WriteLine("    a   b   c   d   e   f   g   h\n");
                for (int i = 0; i < 8; i++)
                {
                    Console.Write(" " + (8 - i) + "  ");
                    for (int j = 0; j < 8; j++)
                    {
                        if (!(board[i, j] is emptyPiece) && !board[i, j].ToString().Contains('W'))
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(board[i, j] + "  ");
                        Console.ResetColor();
                    }
                    Console.WriteLine("\n");
                }
                if (pieceCaptured != null || checkDeclaration != "")
                    printDeclaration();
            }
            public Piece[,] copyBoard(Piece[,] board)
            {
                Piece[,] newBoardCopy = new Piece[8, 8];
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        newBoardCopy[i, j] = new Piece(i, j).copy(board[i, j]);
                        if (newBoardCopy[i, j] is King)
                            if (newBoardCopy[i, j].isPieceWhite())
                                referenceToWhiteKing = (King)newBoardCopy[i, j];
                            else
                                referenceToBlackKing = (King)newBoardCopy[i, j];
                    }                 
                return newBoardCopy;
            }
            public void movePiece(Piece destination, Piece current)
            {
                if (playerChooseToPerformSmallCastling || playerChooseToPerformBigCastling)
                {
                    performCastling();
                    return;
                }
                currentRowCopy = current.row;
                currentColumnCopy = current.column;
                if (!(destination is emptyPiece) && checkDeclaration == "")
                    pieceCaptured = destination;
                board[destination.row, destination.column] = current;
                if (pieceCaptured != null || (checkDeclaration != "" && !(destination is emptyPiece)))
                    board[current.row, current.column] = new emptyPiece(current.row, current.column);
                else
                    board[current.row, current.column] = destination;
                board[destination.row, destination.column].row = destination.row;
                board[destination.row, destination.column].column = destination.column;
                board[currentRowCopy, currentColumnCopy].row = currentRowCopy;
                board[currentRowCopy, currentColumnCopy].column = currentColumnCopy;
                if (current is King)
                {
                    if (current.isPieceWhite())
                        referenceToWhiteKing = (King)board[current.row, current.column];
                    else
                        referenceToBlackKing = (King)board[current.row, current.column];
                }
            }
            public Piece convertInputIntoPiece(string move)
            {
                if (move == "small" || move == "big" || move == "forCastling")
                {
                    if (move == "small") playerChooseToPerformSmallCastling = true;
                    if (move == "big") playerChooseToPerformBigCastling = true;
                    return new emptyPiece(0, 0);
                }
                int row = 0;
                int[] index = { 8, 7, 6, 5, 4, 3, 2, 1 };
                for (int i = 0; i < 8; i++)
                    if (int.Parse("" + move[1]) == index[i])
                        row = i;
                int column = 0;
                string charIndex = "abcdefgh";
                for (int i = 0; i < 8; i++)
                    if (move[0] == charIndex[i])
                        column = i;
                return board[row, column];
            }  
            public void capture(Piece destination, Piece current)
            {
                board[destination.row, destination.column] = current;
                board[current.row, current.column] = new emptyPiece(current.row, current.column);
            }
            public void performCastling()
            {
                if (playerChooseToPerformSmallCastling)
                {
                    movePiece(board[isWhiteTurn ? 7 : 0, 6], isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing);
                    movePiece(board[isWhiteTurn ? 7 : 0, 5], board[isWhiteTurn ? 7 : 0, 7]);
                }
                else
                {
                    movePiece(board[isWhiteTurn ? 7 : 0, 2], isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing);
                    movePiece(board[isWhiteTurn ? 7 : 0, 3], board[isWhiteTurn ? 7 : 0, 0]);
                }
            }
            public void performEnPassat(Piece destination, Piece current)
            {
                if (destination.column > current.column)
                    movePiece(board[current.isPieceWhite() ? current.row - 1 : current.row + 1, current.column + 1], current);                    
                if (destination.column < current.column)
                    movePiece(board[current.isPieceWhite() ? current.row - 1 : current.row + 1, current.column - 1], current);
                pieceCaptured = board[current.isPieceWhite() ? current.row + 1 : current.row - 1, current.column];
                board[current.isPieceWhite() ? current.row + 1 : current.row - 1, current.column] = new emptyPiece(current.isPieceWhite() ? current.row + 1 : current.row - 1, current.column);
                isEnPassatThisTurn = true;
            }
            public void printDeclaration()
            {
                if (checkDeclaration != "")
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(checkDeclaration);
                    Console.ResetColor();
                    checkDeclaration = "";
                }
                if (pieceCaptured != null)
                {
                    if (!pieceCaptured.isPieceWhite())
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(pieceCaptured);
                    Console.ResetColor();
                    Console.WriteLine(" captured!\n");
                    pieceCaptured = null;
                }
            }

            public bool isCheck()
            {
                if (playerChooseToPerformSmallCastling || playerChooseToPerformBigCastling)
                    return false;
                for(int row = 0; row < 8; row++)
                    for(int column = 0; column < 8; column++)
                        if (isMoveValid(isWhiteTurn ? referenceToBlackKing : referenceToWhiteKing, board[row, column]))
                            return true;                                  
                return false;
            }
            public bool isCheck(Piece emptySquare)
            {
                for (int row = 0; row < 8; row++)
                    for (int column = 0; column < 8; column++)
                    {
                        invalidDeclaration = "";
                        if (board[row, column] is emptyPiece && board[row, column].isPieceWhite()
                            == isWhiteTurn && isMoveValid(emptySquare, board[row, column]))
                            return true;
                    }                       
                invalidDeclaration = "";
                return false;
            }
            public bool isCheckMate()
            {
                if (checkDeclaration == "")
                    return false;
                bool isCheckk = false;
                for (int row = 0; row < 8; row++)
                    for (int column = 0; column < 8; column++)
                        for (int destinationRow = 0; destinationRow < 8; destinationRow++)
                            for (int destinationColumn = 0; destinationColumn < 8; destinationColumn++)
                            {
                                invalidDeclaration = "";
                                if (board[row, column].isPieceWhite() != isWhiteTurn
                                    && isMoveValid(board[destinationRow, destinationColumn], board[row, column], true))
                                {
                                    boardCopy = copyBoard(board);
                                    movePiece(board[destinationRow, destinationColumn], board[row, column]);
                                    isCheckk = isCheck();
                                    board = copyBoard(boardCopy);
                                    if (!isCheckk)
                                        return false;
                                }
                            }
                pieceCaptured = null;
                return true;
            }

            public bool isDraw()
            {
                if (isStalemate() || !isSufficientMaterial() || isThreeFoldRepetitionApply() || fiftyMovesCount == 50)
                {
                    Console.WriteLine("Its a draw. GAME OVER!");
                    return true;
                }
                return false;   
            }
            public bool isStalemate()
            {
                for (int row = 0; row < 8; row++)
                    for (int column = 0; column < 8; column++)
                        for (int destinationRow = 0; destinationRow < 8; destinationRow++)
                            for (int destinationColumn = 0; destinationColumn < 8; destinationColumn++)
                                if (isMoveValid(board[destinationRow, destinationColumn], board[row, column], true) &&
                                    doesMoveSavesKing(board[destinationRow, destinationColumn], board[row, column]))
                                    return false;
                return true;
            }
            public bool isSufficientMaterial()
            {
                Piece[] piecesLeftOnBoard = new Piece[3];
                for (int row = 0, k= 0; row < 8; row++)
                    for (int column = 0; column < 8; column++)
                        if (!(board[row, column] is emptyPiece))
                        {
                            piecesLeftOnBoard[k] = board[row, column];
                            k++;
                            if (k == 3)
                                return true;
                        }
                if (piecesLeftOnBoard[0] is King && piecesLeftOnBoard[1] is King && piecesLeftOnBoard[2] == null)
                    return false;
                if ((piecesLeftOnBoard[0] is King && piecesLeftOnBoard[1] is King && piecesLeftOnBoard[2] is Bishop) ||
                        (piecesLeftOnBoard[0] is King && piecesLeftOnBoard[1] is Bishop && piecesLeftOnBoard[2] is King) ||
                        (piecesLeftOnBoard[0] is Bishop && piecesLeftOnBoard[1] is King && piecesLeftOnBoard[2] is King) ||
                        (piecesLeftOnBoard[0] is King && piecesLeftOnBoard[1] is King && piecesLeftOnBoard[2] is Knight) ||
                        (piecesLeftOnBoard[0] is King && piecesLeftOnBoard[1] is Knight && piecesLeftOnBoard[2] is King) ||
                        (piecesLeftOnBoard[0] is Knight && piecesLeftOnBoard[1] is King && piecesLeftOnBoard[2] is King))
                    return false; ;
                return true;
            }
            public bool isThreeFoldRepetitionApply()
            {
                int repetitionCount;
                for (int i = 0; i < historyIndex; i++)
                {
                    repetitionCount = 0;
                    for (int j = 0; j < historyIndex; j++)
                    {
                        if (boardHistory[i].Equals(boardHistory[j]))
                            repetitionCount++;
                        if (repetitionCount == 3)
                            return true;
                    }
                }
                return false;
            }

            public string[] getValidInput()
            {
                string[] input = { };
                bool valid = false;
                Console.Write(isWhiteTurn ? "White," : "Black,");
                Console.WriteLine(" enter your next move and press Enter: ");
                while (!valid)
                {
                    input = Console.ReadLine().ToLower().Split();
                    if ((input[0] == "big" || input[0] == "small") && input[1] == "castling")
                        return new string[] { input[0], input[1], "forCastling" };
                    string chars = "abcdefgh";
                    string nums = "12345678";
                    if (input.Length == 3 && chars.Contains(input[0][0]) && chars.Contains(input[2][0]) &&
                        nums.Contains(input[0][1]) && nums.Contains(input[2][1]) && input[1] == "to")
                        valid = true;
                    else
                    {
                        Console.WriteLine("\nInvalid input, try again.");
                        Console.WriteLine("\nreminder:\nPlayer makes his move by entering:\n 1. current position of the piece he wants to play\n" +
                    " 2. 'to'\n 3. the position he wants to move it to\nThe move should be in the folowing format: char# to char#," +
                    " example: d7 to d5.\nFor castling enter: big/small castling.");
                        Console.Write(isWhiteTurn ? "\nWhite," : "\nBlack,");
                        Console.WriteLine(" enter your next move and press Enter: ");
                    }
                }
                return input;
            }     
            public bool isMoveValid(Piece destination, Piece current)
            {
                if (playerChooseToPerformSmallCastling)
                {
                    if (!isSmallCastlingValid())
                    {
                        invalidDeclaration = "Castling is not allowed in this situation";
                        return false;
                    }
                    return true;
                }
                if (playerChooseToPerformBigCastling)
                {
                    if (!isBigCastlingValid())
                    {
                        invalidDeclaration = "Castling is not allowed in this situation";
                        return false;
                    }
                    return true;
                }
                if (!isMoveValid(destination, current, false))
                    return false;
                if (current.ToString().Contains('W') && !isWhiteTurn ||
                   !current.ToString().Contains('W') && isWhiteTurn)
                {
                    invalidDeclaration = "You can only move your own piece";
                    return false;
                }
                return true;
            }
            public bool isMoveValid(Piece destination, Piece current, bool isMate)
            {
                if (current is emptyPiece)
                {
                    invalidDeclaration = "You can't move an empty square";
                    return false;
                }
                if (destination == current)
                {
                    invalidDeclaration = "You have to move the piece";
                    return false;
                }    
                if (!current.isMoveValidForPiece(destination, board))
                {
                    invalidDeclaration = "This move is not valid";
                    return false;
                }
                if (current.isPieceBlockingTheWay(destination, board))
                {
                    invalidDeclaration = "Piece cannot move through other piece";
                    return false;
                }    
                if (!(destination is emptyPiece))
                    if (destination.isPieceWhite() == current.isPieceWhite())
                    {
                        invalidDeclaration = "You can not move youe piece to this square";
                        return false;
                    }
                if ((current is Pawn) && destination.column != current.column && destination is emptyPiece)
                {
                    if (!isEnPassatLegal(destination, current))
                    {
                        invalidDeclaration = "This move is illegal";
                        return false;
                    }
                    isEnPassatThisTurn = true;
                }
                return true;
            }
            public bool isEnPassatLegal(Piece destination, Piece current)
            {
                if (current.isValidSquare(currentRowCopy, currentColumnCopy + 1) && board[currentRowCopy, currentColumnCopy + 1] is Pawn
                    && ((Pawn)board[currentRowCopy, currentColumnCopy + 1]).canBeCapturedEnPassant)
                    return true;
                if (current.isValidSquare(currentRowCopy, currentColumnCopy - 1) && board[currentRowCopy, currentColumnCopy - 1] is Pawn
                    && ((Pawn)board[currentRowCopy, currentColumnCopy - 1]).canBeCapturedEnPassant)
                    return true;
                return false;
            }
            public bool isSmallCastlingValid()
            {               
                if (!(isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing).hasntMovedYet || (isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing).isUnderThreat
                    || (isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing).hasAlreadyBeenThreatened)
                    return false;
                if (!(board[isWhiteTurn? 7 : 0, 5] is emptyPiece) || !(board[isWhiteTurn? 7 : 0, 6] is emptyPiece))
                    return false;
                if (!(board[isWhiteTurn ? 7 : 0, 7] is Rook) || !((Rook)board[isWhiteTurn ? 7 : 0, 7]).hasntMovedYet)
                    return false;
                // are the relevant squares threatened?
                if (isCheck(board[isWhiteTurn ? 7 : 0, 5]) || isCheck(board[isWhiteTurn ? 7 : 0, 6]))
                    return false;
                // will king be under threat after castling?
                movePiece(board[isWhiteTurn ? 7 : 0, 6], isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing);
                movePiece(board[isWhiteTurn ? 7 : 0, 5], board[isWhiteTurn ? 7 : 0, 7]);
                if (isCheck())
                {
                    movePiece(isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing, board[isWhiteTurn ? 7 : 0, 6]);
                    movePiece(board[isWhiteTurn ? 7 : 0, 7], board[isWhiteTurn ? 7 : 0, 5]);
                    return false;
                }
                movePiece(isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing, board[isWhiteTurn ? 7 : 0, 6]);
                movePiece(board[isWhiteTurn ? 7 : 0, 7], board[isWhiteTurn ? 7 : 0, 5]);
                invalidDeclaration = "";
                return true;
            }
            public bool isBigCastlingValid()
            {
                if(!(isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing).hasntMovedYet || (isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing).isUnderThreat
                    || (isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing).hasAlreadyBeenThreatened)
                    return false;
                if (!(board[isWhiteTurn ? 7 : 0, 3] is emptyPiece) || !(board[isWhiteTurn ? 7 : 0, 2] is emptyPiece) || !(board[isWhiteTurn ? 7 : 0, 1] is emptyPiece))
                    return false;
                if (!(board[isWhiteTurn ? 7 : 0, 0] is Rook) || !((Rook)board[isWhiteTurn ? 7 : 0, 0]).hasntMovedYet)
                    return false;
                // are the relevant squares threatened?
                if (isCheck(board[isWhiteTurn ? 7 : 0, 3]) || isCheck(board[isWhiteTurn ? 7 : 0, 2]))
                    return false;
                // will king be under threat after castling?
                movePiece(board[isWhiteTurn ? 7 : 0, 2], isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing);
                movePiece(board[isWhiteTurn ? 7 : 0, 3], board[isWhiteTurn ? 7 : 0, 0]);
                if (isCheck())
                {
                    movePiece(isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing, board[isWhiteTurn ? 7 : 0, 2]);
                    movePiece(board[isWhiteTurn ? 7 : 0, 0], board[isWhiteTurn ? 7 : 0, 3]);
                    return false;
                }
                movePiece(isWhiteTurn ? referenceToWhiteKing : referenceToBlackKing, board[isWhiteTurn ? 7 : 0, 2]);
                movePiece(board[isWhiteTurn ? 7 : 0, 0], board[isWhiteTurn ? 7 : 0, 3]);
                invalidDeclaration = "";
                return true;
            }           
            public bool doesMoveSavesKing(Piece destination, Piece current)
            {
                if (playerChooseToPerformSmallCastling || playerChooseToPerformBigCastling)
                    return true;
                isWhiteTurn = isWhiteTurn ? false : true;
                boardCopy = copyBoard(board);
                movePiece(destination, current);
                if (isCheck())
                {
                    isWhiteTurn = isWhiteTurn ? false : true;
                    board = copyBoard(boardCopy);
                    destination = board[current.row, current.column];
                    current = board[currentRowCopy, currentColumnCopy];
                    return false;
                }
                isWhiteTurn = isWhiteTurn ? false : true;
                board = copyBoard(boardCopy);
                destination.row = current.row;
                destination.column = current.column;
                current.row = currentRowCopy;
                current.column = currentColumnCopy;
                return true;
            }

            public void resetProperties(Piece destination, Piece current)
            {
                if (checkDeclaration != "")
                {
                    (isWhiteTurn ? referenceToBlackKing : referenceToWhiteKing).isUnderThreat = true;
                    (isWhiteTurn ? referenceToBlackKing : referenceToWhiteKing).hasAlreadyBeenThreatened = true;
                }
                checkDeclaration = "";
                fiftyMovesCount++;
                if (current is Pawn || pieceCaptured != null)
                {
                    fiftyMovesCount = 0;
                    boardHistory = new Piece[50][,];
                    historyIndex = 0;
                }
                resetEnPasant(destination, current);
                resetFirstMoves(current);
                invalidDeclaration = "";
                isEnPassatThisTurn = false;
                playerChooseToPerformBigCastling = false;
                playerChooseToPerformSmallCastling = false;
                isWhiteTurn = isWhiteTurn ? false : true;
                boardHistory[historyIndex] = boardCopy;
                historyIndex++;
            }
            public void resetEnPasant(Piece destination, Piece current)
            {
                for (int i = 0; i < board.GetLength(0); i++)
                    for (int j = 0; j < board.GetLength(1); j++)
                        if (board[i, j] is Pawn)
                            ((Pawn)board[i, j]).canBeCapturedEnPassant = false;
                if (current is Pawn)
                {
                    if (current.isPieceWhite() && current.row == currentRowCopy - 2)
                        ((Pawn)current).canBeCapturedEnPassant = true;
                    if (!current.isPieceWhite() && current.row == currentRowCopy + 2)
                        ((Pawn)board[current.row, current.column]).canBeCapturedEnPassant = true;
                    fiftyMovesCount = 0;
                }
            }
            public void resetFirstMoves(Piece current)
            {
                if (current is King)
                    ((King)current).hasntMovedYet = false;
                if (current is Rook)
                    ((Rook)current).hasntMovedYet = false;
                if (current is Pawn)
                    ((Pawn)board[current.row, current.column]).hasntMovedYet = false;
            }
        }

        class Piece
        {
            public int row;
            public int column;
            public Piece(int row, int column)
            {
                this.row = row;
                this.column = column;
            }
            public virtual bool isMoveValidForPiece(Piece destination, Piece[,] board)
            {
                return false;
            }
            public virtual bool isPieceBlockingTheWay(Piece destination, Piece[,] board)
            {
                return false;
            }
            public bool isValidSquare(int row, int column)
            {
                if (row < 0 || row > 7)
                    return false;
                if (column < 0 || column > 7)
                    return false;
                return true;
            }
            public virtual Piece copy(Piece piece)
            {
                if (piece is Pawn)
                   return ((Pawn)piece).copy(piece);
                if (piece is King)
                    return ((King)piece).copy(piece);
                if (piece is Queen)
                    return ((Queen)piece).copy(piece);
                if (piece is Bishop)
                    return ((Bishop)piece).copy(piece);
                if (piece is Rook)
                    return ((Rook)piece).copy(piece);
                if (piece is Knight)
                    return ((Knight)piece).copy(piece);
                return ((emptyPiece)piece).copy(piece);
            }
            public bool isPieceWhite()
            {
                if (this is Pawn)
                    return ((Pawn)this).isWhite;
                if (this is King)
                    return ((King)this).isWhite;
                if (this is Queen)
                    return ((Queen)this).isWhite;
                if (this is Bishop)
                    return ((Bishop)this).isWhite;
                if (this is Rook)
                    return ((Rook)this).isWhite;
                if (this is Knight)
                    return ((Knight)this).isWhite;
                return true;
            }

        }

        class King : Piece
        {
            public bool isWhite;
            public bool hasntMovedYet;
            public bool isUnderThreat;
            public bool hasAlreadyBeenThreatened;
            public King(int row, int column, bool pieceColor) : base(row, column)
            {
                this.isWhite = pieceColor;
                this.hasntMovedYet = true;
                this.hasAlreadyBeenThreatened = false;
                this.isUnderThreat = false;
        }
            public override bool isMoveValidForPiece(Piece destination, Piece[,] board)
            {
                if ((destination.row == this.row + 1) && (destination.column == this.column + 1 || destination.column == this.column - 1 || destination.column == this.column))
                    return true;
                if ((destination.row == this.row - 1) && (destination.column == this.column + 1 || destination.column == this.column - 1 || destination.column == this.column))
                    return true;
                if (destination.row == this.row && (destination.column == this.column + 1 || destination.column == this.column - 1))
                    return true;
                return false;
            }
            public override Piece copy(Piece piece)
            {
                King pieceCopy = new King(row, column, isWhite);
                pieceCopy.hasntMovedYet = hasntMovedYet;
                pieceCopy.hasAlreadyBeenThreatened = hasAlreadyBeenThreatened;
                pieceCopy.isUnderThreat = isUnderThreat;
                return pieceCopy;
            }
            public override string ToString()
            {
                return "K" + (isWhite ? "W" : "B");
            }
        }

        class Queen : Piece
        {
            public bool isWhite;
            public Queen(int row, int column, bool pieceColor) : base(row, column)
            {
                this.isWhite = pieceColor;
            }
            public override bool isMoveValidForPiece(Piece destination, Piece[,] board)
            {
                if(destination.row == this.row || destination.column == this.column)
                    return true;
                if (destination.row < this.row)
                    for (int i = this.row - 1, j = 1; i >= destination.row; i--, j++)
                        if (destination.row == i && (destination.column == this.column - j || destination.column == this.column + j))
                            return true;
                if (destination.row > this.row)
                    for (int i = this.row + 1, j = 1; i <= destination.row; i++, j++)
                        if (destination.row == i && (destination.column == this.column - j || destination.column == this.column + j))
                            return true;
                return false;
            }
            public override bool isPieceBlockingTheWay(Piece destination, Piece[,] board)
            {
                if (destination.row < this.row && destination.column < this.column)
                    for (int i = this.row - 1, j = 1; i > destination.row; i--, j++)
                        if (board[i, this.column - j].ToString() != " -")
                            return true;
                if (destination.row < this.row && destination.column > this.column)
                    for (int i = this.row - 1, j = 1; i > destination.row; i--, j++)
                        if (board[i, this.column + j].ToString() != " -")
                            return true;
                if (destination.row > this.row && destination.column < this.column)
                    for (int i = this.row + 1, j = 1; i < destination.row; i++, j++)
                        if (board[i, this.column - j].ToString() != " -")
                            return true;
                if (destination.row > this.row && destination.column > this.column)
                    for (int i = this.row + 1, j = 1; i < destination.row; i++, j++)
                        if (board[i, this.column + j].ToString() != " -")
                            return true;
                if (destination.row == this.row && destination.column < this.column)
                    for (int i = this.column - 1; i > destination.column; i--)
                        if (board[destination.row, i].ToString() != " -")
                            return true;
                if (destination.row == this.row && destination.column > this.column)
                    for (int i = this.column + 1; i < destination.column; i++)
                        if (board[destination.row, i].ToString() != " -")
                            return true;
                if (destination.column == this.column && destination.row < this.row)
                    for (int i = this.row - 1; i > destination.row; i--)
                        if (board[i, destination.column].ToString() != " -")
                            return true;
                if (destination.column == this.column && destination.row > this.row)
                    for (int i = this.row + 1; i < destination.row; i++)
                        if (board[i, destination.column].ToString() != " -")
                            return true;
                return false;
            }
            public override Piece copy(Piece piece)
            {
                return new Queen(row, column, isWhite);
            }
            public override string ToString()
            {
                return "Q" + (isWhite ? "W" : "B");
            }
        }

        class Knight : Piece
        {
            public bool isWhite;
            public Knight(int row, int column, bool pieceColor) : base(row, column)
            {
                this.isWhite = pieceColor;
            }
            public override bool isMoveValidForPiece(Piece destination, Piece[,] board)
            {
                if ((destination.row == this.row + 1 || destination.row == this.row - 1) &&
                   (destination.column == this.column - 2 || destination.column == this.column + 2))
                    return true;
                if ((destination.row == this.row + 2 || destination.row == this.row - 2) &&
                    (destination.column == this.column - 1 || destination.column == this.column + 1))
                    return true;
                return false;
            }
            public override Piece copy(Piece piece)
            {
                return new Knight(row, column, isWhite);
            }
            public override string ToString()
            {
                return "N" + (isWhite ? "W" : "B");
            }
        }

        class Bishop : Piece
        {
            public bool isWhite;
            public Bishop(int row, int column, bool pieceColor) : base(row, column)
            {
                this.isWhite = pieceColor;
            }
            public override bool isMoveValidForPiece(Piece destination, Piece[,] board)
            {
                if (destination.row < this.row)
                    for (int i = this.row - 1, j = 1; i >= destination.row; i--, j++)
                        if (destination.row == i && (destination.column == this.column - j || destination.column == this.column + j))
                            return true;
                if (destination.row > this.row)
                    for (int i = this.row + 1, j = 1; i <= destination.row; i++, j++)
                        if (destination.row == i && (destination.column == this.column - j || destination.column == this.column + j))
                            return true;
                return false;
            }
            public override bool isPieceBlockingTheWay(Piece destination, Piece[,] board)
            {
                if (destination.row < this.row && destination.column < this.column)
                    for (int i = this.row - 1, j = 1; i > destination.row; i--, j++)
                        if (destination.isValidSquare(i, this.column - j) && board[i, this.column - j].ToString() != " -")
                            return true;
                if (destination.row < this.row && destination.column > this.column)
                    for (int i = this.row - 1, j = 1; i > destination.row; i--, j++)
                        if (destination.isValidSquare(i, this.column + j) && board[i, this.column + j].ToString() != " -")
                            return true;
                if (destination.row > this.row && destination.column < this.column)
                    for (int i = this.row + 1, j = 1; i < destination.row; i++, j++)
                        if (destination.isValidSquare(i, this.column - j) && board[i, this.column - j].ToString() != " -")
                            return true;
                if (destination.row > this.row && destination.column > this.column)
                    for (int i = this.row + 1, j = 1; i < destination.row; i++, j++)
                        if (destination.isValidSquare(i, this.column + j) && board[i, this.column + j].ToString() != " -")
                            return true;
                return false;
            }
            public override Piece copy(Piece piece)
            {
                return new Bishop(row, column, isWhite);
            }
            public override string ToString()
            {
                return "B" + (isWhite ? "W" : "B");
            }
        }

        class Rook : Piece
        {
            public bool isWhite;
            public bool hasntMovedYet;
            public Rook(int row, int column, bool pieceColor) : base(row, column)
            {
                isWhite = pieceColor;
                hasntMovedYet = true;
            }
            public override bool isMoveValidForPiece(Piece destination, Piece[,] board)
            {
                if (destination.row == this.row || destination.column == this.column)
                    return true;
                return false;
            }
            public override bool isPieceBlockingTheWay(Piece destination, Piece[,] board)
            {
                if (destination.column == this.column && destination.row < this.row)
                    for (int i = this.row - 1; i > destination.row; i--)
                        if (board[i, destination.column].ToString() != " -")
                            return true;
                if (destination.column == this.column && destination.row > this.row)
                    for (int i = this.row + 1; i < destination.row; i++)
                        if (board[i, destination.column].ToString() != " -")
                            return true;
                if (destination.row == this.row && destination.column < this.column)
                    for (int i = this.column - 1; i > destination.column; i--)
                        if (board[destination.row, i].ToString() != " -")
                            return true;
                if (destination.row == this.row && destination.column > this.column)
                    for (int i = this.column + 1; i < destination.column; i++)
                        if (board[destination.row, i].ToString() != " -")
                            return true;
                return false;
            }
            public override Piece copy(Piece piece)
            {
                Rook pieceCopy = new Rook(row, column, isWhite);
                pieceCopy.hasntMovedYet = hasntMovedYet;
                return pieceCopy;
            }
            public override string ToString()
            {
                return "R" + (isWhite ? "W" : "B");
            }
        }

        class Pawn : Piece
        {
            public bool isWhite;
            public bool hasntMovedYet;
            public bool canBeCapturedEnPassant;
            public Pawn(int row, int column, bool pieceColor) : base(row, column)
            {
                isWhite = pieceColor;
                hasntMovedYet = true;
                canBeCapturedEnPassant = false;
            }
            public override bool isMoveValidForPiece(Piece destination, Piece[,] board)
            {
                bool result = false;
                if (isWhite && destination.row == row - 2 && destination.column == column && destination is emptyPiece)
                    result = hasntMovedYet ? true : false;
                if (!isWhite && destination.row == row + 2 && destination.column == column && destination is emptyPiece)
                    result = hasntMovedYet ? true : false;
                if (isWhite && destination is emptyPiece && destination.row == row - 1 &&
                    (destination.column == column || destination.column == column + 1 || destination.column == column - 1))
                    result = true;
                if (!isWhite && destination is emptyPiece && destination.row == row + 1 &&
                    (destination.column == column || destination.column == column + 1 || destination.column == column - 1))
                    result = true;
                if (isWhite && !(destination is emptyPiece) && !destination.ToString().Contains('W') && destination.row == row - 1 
                    && (destination.column == column - 1 || destination.column == column + 1))
                    result = true;
                if (!isWhite && !(destination is emptyPiece) && destination.ToString().Contains('W') && destination.row == this.row + 1
                    && (destination.column == column - 1 || destination.column == column + 1))
                    result = true;
                return result;
            }
            public override Piece copy(Piece piece)
            {
                Pawn newPawn = new Pawn(row, column, isWhite);
                newPawn.hasntMovedYet = hasntMovedYet;
                newPawn.canBeCapturedEnPassant = canBeCapturedEnPassant;
                return newPawn;
            }
            public override string ToString()
            {
                return "P" + (isWhite ? "W" : "B");
            }
        }

        class emptyPiece : Piece
        {
            public emptyPiece(int row, int column) : base(row, column) { }
            public override Piece copy(Piece piece)
            {
                return new emptyPiece(row, column);
            }
            public override string ToString()
            {
                return " -";
            }
        }
    }
}
