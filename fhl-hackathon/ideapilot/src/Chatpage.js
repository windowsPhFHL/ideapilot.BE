import React, { useState } from "react";
import "./ChatPage.css";

const ChatPage = () => {
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");

  const sendMessage = () => {
    if (input.trim() === "") return;

    // Alternate messages left and right
    const newMessage = {
      text: input,
      isLeft: messages.length % 2 === 0, // Alternates messages
    };

    setMessages([...messages, newMessage]);
    setInput(""); // Clear input field
  };

  return (
    <div className="chat-container">
      {/* Sidebar */}
      <div className="sidebar">Workspace, Projects, etc.</div>

      {/* Main Content */}
      <div className="content">
        {/* Chat Area (where messages appear) */}
        <div className="chat-box">
          {messages.map((msg, index) => (
            <div
              key={index}
              className={`chat-message ${msg.isLeft ? "left" : "right"}`}
            >
              {msg.text}
            </div>
          ))}
        </div>

        {/* Suggested Prompts */}
        <div className="suggested-prompts-container">
          <div className="suggested-prompts">
            <div className="prompt-card">Pproject context</div>
            <div className="prompt-card">System design</div>
            <div className="prompt-card">ADO tasks</div>
            <div className="prompt-card">drag and drop</div>
          </div>
        </div>

        {/* Prompt Input */}
        <div className="prompt-input-container">
          <button className="plus-button">➕</button>
          <input
            type="text"
            className="prompt-input"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Type a message..."
          />
          <button className="send-button" onClick={sendMessage}>➡️</button>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
