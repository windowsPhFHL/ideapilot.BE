import { useState } from "react";
import { TextField, IconButton } from "@mui/material";
import { Add } from "@mui/icons-material";

export default function ChatArea({ selectedItem }) {
  const [message, setMessage] = useState("");

  return (
    <main className="flex-1 flex flex-col justify-between p-4 bg-gray-100">
      <div className="flex-1 overflow-auto p-4">
        <p className="text-gray-500">{selectedItem}</p>
      </div>

      <div className="flex items-center bg-white p-2 rounded-lg shadow-md">
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Type a message..."
          value={message}
          onChange={(e) => setMessage(e.target.value)}
        />
        <IconButton className="ml-2" color="primary">
          <Add />
        </IconButton>
      </div>
    </main>
  );
}
