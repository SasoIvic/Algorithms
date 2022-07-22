import bayer
import unittest
import numpy as np


class Test(unittest.TestCase):
    def test_interpolacija_bggr(self):
        podatki = np.array([[2, 6, 2, 8],
                            [4, 8, 6, 6],
                            [8, 2, 4, 4],
                            [6, 4, 8, 2]], dtype=np.uint8)

        resitev_bggr = np.zeros((4,4,3), dtype=np.uint8)
        resitev_bggr[:, :, 0] = [[8, 8, 7, 6], 
                                 [8, 8, 7, 6],
                                 [6, 6, 5, 4],
                                 [4, 4, 3, 2]]
        resitev_bggr[:, :, 1] = [[5, 6, 6, 8], 
                                 [4, 4, 6, 6],
                                 [4, 2, 5, 4],
                                 [6, 5, 8, 6]]
        resitev_bggr[:, :, 2] = [[2, 2, 2, 2], 
                                 [5, 4, 3, 3],
                                 [8, 6, 4, 4],
                                 [8, 6, 4, 4]]

        rezultat = bayer.bayer_v_rgb(podatki, 'BGGR', True)
        self.assertIsInstance(rezultat, np.ndarray)
        self.assertEqual(rezultat.dtype, np.uint8)
        np.testing.assert_array_equal(rezultat, resitev_bggr)

if __name__ == '__main__':
    unittest.main(verbosity=2)
