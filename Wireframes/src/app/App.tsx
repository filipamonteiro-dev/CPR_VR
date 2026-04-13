import { RouterProvider } from "react-router";
import { router } from "./routes";
import "../styles/fonts.css";

export default function App() {
  return (
    <div style={{ fontFamily: "'Space Mono', monospace" }} className="size-full">
      <RouterProvider router={router} />
    </div>
  );
}
