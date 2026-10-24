import { Route, Routes } from 'react-router';
import NavMenu from './components/NavMenu.jsx';
import Home from './pages/Home.jsx';
import Counter from './pages/Counter.jsx';
import NotFound from './pages/NotFound.jsx';

export default function App() {
  return (
    <>
      <NavMenu />
      <main>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/counter" element={<Counter />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
    </>
  );
}
