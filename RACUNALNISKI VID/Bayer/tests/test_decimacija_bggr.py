import bayer
import unittest
import numpy as np

class Test(unittest.TestCase):
    def test_decimacija_bggr(self):
        podatki = np.array(
            [[110, 150, 220, 230, 110, 180],
             [120, 160, 210, 240, 150, 140],
             [130, 170, 200, 250, 120, 170],
             [140, 180, 190, 250, 160, 130]], dtype=np.uint8)

        resitev_bggr = np.array(
            [[[160, (150+120)//2, 110], 
              [240, (230+210)//2, 220], 
              [140, (180+150)//2, 110]], 
             [[180, (170+140)//2, 130], 
              [250, (250+190)//2, 200], 
              [130, (170+160)//2, 120]]], dtype=np.uint8)

        rezultat = bayer.bayer_v_rgb(podatki, 'BGGR')
        self.assertIsInstance(rezultat, np.ndarray)
        self.assertEqual(rezultat.dtype, np.uint8)
        np.testing.assert_array_equal(rezultat, resitev_bggr)

if __name__ == '__main__':
    unittest.main(verbosity=2)
