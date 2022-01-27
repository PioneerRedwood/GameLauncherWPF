CREATE TABLE MESSENGER
(
	SENDER_ID INT NOT NULL,
    RECEIVER_ID INT NOT NULL,
    SENT_DATETIME DATETIME(2) NOT NULL,
    CONTENTS VARCHAR(512) NOT NULL,
    
    CONSTRAINT MESSENGER_PK PRIMARY KEY(SENDER_ID, RECEIVER_ID, SENT_DATETIME),
    CONSTRAINT MESSENGER_SENDER_FK FOREIGN KEY(SENDER_ID) REFERENCES RED_USER(USER_ID),
    CONSTRAINT MESSENGER_RECEIVER_FK FOREIGN KEY(RECEIVER_ID) REFERENCES RED_USER(USER_ID)
);

SELECT * FROM MESSENGER ORDER BY SENT_DATETIME;

SELECT * FROM MESSENGER
WHERE (SENDER_ID = 6 AND RECEIVER_ID = 7) OR (SENDER_ID = 7 AND RECEIVER_ID = 6)
ORDER BY SENT_DATETIME;

DROP TABLE MESSENGER;

INSERT INTO MESSENGER (SENDER_ID, RECEIVER_ID, SENT_DATETIME, CONTENTS)
VALUES (9, 7, NOW(2), "HELLO WORLD!");

INSERT INTO MESSENGER (SENDER_ID, RECEIVER_ID, SENT_DATETIME, CONTENTS)
VALUES (9, 6, NOW(2), "HI ?");


INSERT INTO MESSENGER (SENDER_ID, RECEIVER_ID, SENT_DATETIME, CONTENTS)
VALUES (9, 8, NOW(2), "HI 8");

SELECT * FROM MESSENGER WHERE (SEND_ID = 9 AND RECEIVER_ID = 6) OR (SEND_ID = 6 AND RECEIVER_ID = 9) ORDER BY SENT_DATETIME;

INSERT INTO MESSENGER (SENDER_ID, RECEIVER_ID, SENT_DATETIME, CONTENTS) VALUES(9, 7, NOW(2), "Hi?");