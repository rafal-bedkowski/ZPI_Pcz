import { Link, NavLink } from 'react-router';

export default function NavMenu() {
  return (
    <nav className="navbar" aria-label="Menu główne">
      <Link to="/" className="brand">WebStore</Link>
      <ul className="menu">
        <li><NavLink to="/" end>Home</NavLink></li>
        <li><NavLink to="/counter">Counter</NavLink></li>
      </ul>
    </nav>
  );
}
