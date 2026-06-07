import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { Provider } from "react-redux";
import { store } from "./app/store";
import App from "./App";
import { Toaster } from "react-hot-toast";
import "./index.css";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <Provider store={store}>
      <BrowserRouter>
        <Toaster
          position="top-center"
          toastOptions={{
            style: {
              background: "#1A1D24", // dark-800
              color: "#fff",
              border: "1px solid #2A2E39", // dark-700
            },
            success: {
              iconTheme: {
                primary: "#22c55e", // accent-success
                secondary: "#fff",
              },
            },
            error: {
              iconTheme: {
                primary: "#ef4444", // accent-danger
                secondary: "#fff",
              },
            },
          }}
        />
        <App />
      </BrowserRouter>
    </Provider>
  </React.StrictMode>,
);
